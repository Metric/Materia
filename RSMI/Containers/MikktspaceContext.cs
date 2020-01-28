/**
 *  Original C Code:
 *  Copyright (C) 2011 by Morten S. Mikkelsen
 *
 *  This software is provided 'as-is', without any express or implied
 *  warranty.  In no event will the authors be held liable for any damages
 *  arising from the use of this software.
 *
 *  Permission is granted to anyone to use this software for any purpose,
 *  including commercial applications, and to alter it and redistribute it
 *  freely, subject to the following restrictions:
 *
 *  1. The origin of this software must not be misrepresented; you must not
 *     claim that you wrote the original software. If you use this software
 *     in a product, an acknowledgment in the product documentation would be
 *     appreciated but is not required.
 *  2. Altered source versions must be plainly marked as such, and must not be
 *     misrepresented as being the original software.
 *  3. This notice may not be removed or altered from any source distribution.
 */

 /**
  * C# Code Version:
  * Copyright (C) 2019 arcanistry
  * Same License as the original C code above
  * 
  * requires OpenTK for vector math.
  * This C# version only supports triangles
  * this was done to speed up code conversion
  * and because game engines use triangles by default
  */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime;
using Materia.Math3D;

namespace RSMI.Containers
{
    public struct SMikkTSpaceContext
    {
        public SMikkTSpaceInterface m_pInterface;
        public object m_pUserdata;
    }

    public struct SMikkTSpaceInterface
    {
        public int m_getNumFaces(ref SMikkTSpaceContext context)
        {
            Mesh m = context.m_pUserdata as Mesh;
            return m.triangles.Count;
        }

        //we return 3 because we triangulate the mesh before we put it through here
        public int m_getNumVerticesOfFace(ref SMikkTSpaceContext context, int iFace)
        {
            return 3;
        }

        public Vector3 m_getPosition(ref SMikkTSpaceContext context, int iFace, int iVert)
        {
            Mesh m = context.m_pUserdata as Mesh;
            return m.GetPosition(iFace, iVert);
        }

        public Vector3 m_getNormal(ref SMikkTSpaceContext context, int iFace, int iVert)
        {
            Mesh m = context.m_pUserdata as Mesh;
            return m.GetNormal(iFace, iVert);
        }

        public Vector2 m_getTexCoord(ref SMikkTSpaceContext context, int iFace, int iVert)
        {
            Mesh m = context.m_pUserdata as Mesh;
            return m.GetUV(iFace, iVert);
        }

        public void m_setTSpaceBasic(ref SMikkTSpaceContext context, float[] fvTangent, float fSign, int iFace, int iVert)
        {
            Mesh m = context.m_pUserdata as Mesh;
            m.SetTangent(fvTangent, fSign, iFace, iVert);
        }

        public void m_setTSpace(ref SMikkTSpaceContext context, float[] fvTangent, float[] fvBiTangent, float fMagS, float fMagT, bool bIsOrientationPreserving, int iFace, int iVert)
        {

        }
    }

    public static class Mikkt
    {
        const int MARK_DEGENERATE = 1;
        const int QUAD_ONE_DEGEN_TRI = 2;
        const int GROUP_WITH_ANY = 4;
        const int ORIENT_PRESERVING = 8;

        struct SSubGroup
        {
            public int iNrFaces;
            public List<int> triMembers;

            public void Init()
            {
                triMembers = new List<int>();
            }
        }

        class SGroup
        {
            public int iNrFaces;
            public int faceOffset;
            public int iVertex;
            public bool bOrientPreservering;
        }

        struct STriInfo
        {
            public int[] faceNeighbors;
            public SGroup[] AssignedGroup;
            public Vector3 vOs, vOt;
            public float fMagS, fMagT;
            public int iOrgFaceNumber;
            public int iFlag, iTSpacesOffs;
            public int[] vert_num;

            public void Init()
            {
                faceNeighbors = new int[3];
                AssignedGroup = new SGroup[3];
                vOs = Vector3.Zero; vOt = Vector3.Zero;
                fMagS = 0; fMagT = 0;
                iOrgFaceNumber = 0;
                iFlag = 0; iTSpacesOffs = 0;
                vert_num = new int[3];
            }
        }

        struct STSpace
        {
            public Vector3 vOs;
            public float fMagS;
            public Vector3 vOt;
            public float fMagT;
            public int iCounter;
            public bool bOrient;
        }

        static int MakeIndex(int iFace, int iVert)
        {
            return (iFace << 2) | (iVert & 0x3);
        }

        static void IndexToData(out int piFace, out int piVert, int iIndexIn)
        {
            piVert = iIndexIn & 0x3;
            piFace = iIndexIn >> 2;
        }

        static STSpace AvgTSpace(ref STSpace pTS0, ref STSpace pTS1)
        {
            STSpace ts_res = new STSpace();

            if(pTS0.fMagS == pTS1.fMagS && pTS0.fMagT == pTS1.fMagT &&
                pTS0.vOs.Equals(pTS1.vOs) && pTS1.vOt.Equals(pTS1.vOt))
            {
                ts_res.fMagS = pTS0.fMagS;
                ts_res.fMagT = pTS0.fMagT;
                ts_res.vOs = pTS0.vOs;
                ts_res.vOt = pTS0.vOt;
            }
            else
            {
                ts_res.fMagS = 0.5f * (pTS0.fMagS + pTS1.fMagS);
                ts_res.fMagT = 0.5f * (pTS0.fMagT + pTS1.fMagT);
                ts_res.vOs = pTS0.vOs + pTS0.vOs;
                ts_res.vOt = pTS0.vOt + pTS0.vOt;

                if(VNotZero(ref ts_res.vOs))
                {
                    ts_res.vOs.Normalize();
                }
                if(VNotZero(ref ts_res.vOt))
                {
                    ts_res.vOt.Normalize();
                }
            }

            return ts_res;
        }

        static bool NotZero (float fX)
        {
            return Math.Abs(fX) > float.Epsilon;
        }

        static bool VNotZero(ref Vector3 v)
        {
            return NotZero(v.X) || NotZero(v.Y) || NotZero(v.Z);
        }

        public static void GenTangents(Mesh m)
        {
            SMikkTSpaceContext ctx = new SMikkTSpaceContext()
            {
                m_pInterface = new SMikkTSpaceInterface(),
                m_pUserdata = m
            };

            genTangSpaceDefault(ref ctx);
        }

        public static void GenTangents(Mesh m, float angularThreshold)
        {
            SMikkTSpaceContext ctx = new SMikkTSpaceContext()
            {
                m_pInterface = new SMikkTSpaceInterface(),
                m_pUserdata = m
            };

            genTangSpace(ref ctx, angularThreshold);
        }

        /// <summary>
        /// this version of Mikktspace only deals with triangles
        /// in order to save sometime in moving the code over to c#
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool genTangSpaceDefault(ref SMikkTSpaceContext context)
        {
            return genTangSpace(ref context, 180.0f);
        }

        /// <summary>
        /// this version of Mikktspace only deals with triangles
        /// in order to save sometime in moving the code over to c#
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool genTangSpace(ref SMikkTSpaceContext context, float fAngularThreshold)
        {
            int[] piTriListIn = null;
            int[] piGroupTrianglesBuffer = null;
            STriInfo[] pTriInfos = null;
            SGroup[] pGroups = null;
            STSpace[] psTspace = null;

            int iNrTrianglesIn = 0, f = 0, t = 0, i = 0;
            int iNrTSPaces = 0, iNrMaxGroups = 0;
            int iNrActiveGroups = 0, index = 0;
            int iNrFaces = context.m_pInterface.m_getNumFaces(ref context);
            float fThresCos = (float)Math.Cos((fAngularThreshold * ((float)Math.PI / 180.0f)));

            for(f = 0; f < iNrFaces; f++)
            {
                int verts = context.m_pInterface.m_getNumVerticesOfFace(ref context, f);
                if (verts == 3) iNrTrianglesIn++;
            }
            if (iNrTrianglesIn <= 0) return false;

            piTriListIn = new int[3 * iNrTrianglesIn];
            pTriInfos = new STriInfo[iNrTrianglesIn];

            iNrTSPaces = GenerateInitialVerticesIndexList(ref pTriInfos, piTriListIn, ref context, iNrTrianglesIn);

            GenerateSharedVerticesIndexList(piTriListIn, ref context, iNrTrianglesIn);

            InitTriInfo(ref pTriInfos, piTriListIn, ref context, iNrTrianglesIn);

            iNrMaxGroups = iNrTrianglesIn * 3;
            pGroups = new SGroup[iNrMaxGroups];

            for(f = 0; f < iNrMaxGroups; f++)
            {
                pGroups[f] = new SGroup();
            }

            piGroupTrianglesBuffer = new int[iNrTrianglesIn * 3];

            try
            {
                iNrActiveGroups = Build4RuleGroups(ref pTriInfos, ref pGroups, piGroupTrianglesBuffer, piTriListIn, iNrTrianglesIn);

                psTspace = new STSpace[iNrTSPaces];
                for(t = 0; t < iNrTSPaces; t++)
                {
                    psTspace[t].vOs.X = 1.0f;
                    psTspace[t].fMagS = 1.0f;
                    psTspace[t].vOt.Y = 1.0f;
                    psTspace[t].fMagT = 1.0f;
                }

                GenerateTSpaces(ref psTspace, ref pTriInfos, pGroups, iNrActiveGroups, piTriListIn, piGroupTrianglesBuffer, fThresCos, ref context);

                index = 0;
                for(f = 0; f < iNrFaces; f++)
                {
                    int verts = context.m_pInterface.m_getNumVerticesOfFace(ref context, f);
                    if (verts != 3) continue;

                    for(i  = 0; i < verts; ++i)
                    {
                        var pSpace = psTspace[index];
                        float[] tang = new float[] { pSpace.vOs.X, pSpace.vOs.Y, pSpace.vOs.Z };
                        float[] bitang = new float[] { pSpace.vOt.X, pSpace.vOt.Y, pSpace.vOt.Z };
                        context.m_pInterface.m_setTSpace(ref context, tang, bitang, pSpace.fMagS, pSpace.fMagT, pSpace.bOrient, f, i);
                        context.m_pInterface.m_setTSpaceBasic(ref context, tang, pSpace.bOrient ? 1 : -1, f, i);
                        ++index;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            return true;
        }

 /// <summary>
 /// ////////////////////////////////////////////////////////////////////////////////////////////
 /// </summary>

        const int g_iCells = 2048;

        struct STmpVert
        {
            public float[] vert;
            public int index;
        }

        static int FindGridCell(float fMin, float fMax, float fVal)
        {
            float fIndex = g_iCells * ((fVal - fMin) / (fMax - fMin));
            int iIndex = (int)fIndex;
            return iIndex < g_iCells ? (iIndex >= 0 ? iIndex : 0) : (g_iCells - 1);
        }

        static void GenerateSharedVerticesIndexList(int[] piTriList_in_and_out, ref SMikkTSpaceContext pContext, int iNrTrianglesIn)
        {
            int[] piHashTable = null, piHashCount = null, piHashOffsets = null, piHashCount2 = null;
            STmpVert[] pTmpVer = null;
            int i = 0, iChannel = 0, k = 0, e = 0;
            int iMaxCount = 0;
            Vector3 vMin = GetPosition(ref pContext, 0), vMax = vMin, vDim = new Vector3();
            float fMin, fMax;

            for(i = 1; i < (iNrTrianglesIn * 3); ++i)
            {
                int index = piTriList_in_and_out[i];
                Vector3 vP = GetPosition(ref pContext, index);
                if (vMin.X > vP.X) vMin.X = vP.X;
                else if (vMax.X < vP.X) vMax.X = vP.X;
                if (vMin.Y > vP.Y) vMin.Y = vP.Y;
                else if (vMax.Y < vP.Y) vMax.Y = vP.Y;
                if (vMin.Z > vP.Z) vMin.Z = vP.Z;
                else if (vMax.Z < vP.Z) vMax.Z = vP.Z;
            }

            vDim = vMax - vMin;
            iChannel = 0;
            fMin = vMin.X; fMax = vMax.X;
            if(vDim.Y > vDim.X && vDim.Y > vDim.Z)
            {
                iChannel = 1;
                fMin = vMin.Y; fMax = vMax.Y;
            }
            else if(vDim.Z > vDim.X)
            {
                iChannel = 2;
                fMin = vMin.Z; fMax = vMax.Z;
            }

            piHashTable = new int[iNrTrianglesIn * 3];
            piHashCount = new int[g_iCells];
            piHashOffsets = new int[g_iCells];
            piHashCount2 = new int[g_iCells];

            try
            {
                for (i = 0; i < iNrTrianglesIn * 3; ++i)
                {
                    int index = piTriList_in_and_out[i];
                    Vector3 vP = GetPosition(ref pContext, index);
                    float fVal = iChannel == 0 ? vP.X : (iChannel == 1) ? vP.Y : vP.Z;
                    int iCell = FindGridCell(fMin, fMax, fVal);
                    piHashCount[iCell]++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            piHashOffsets[0] = 0;
            for(k = 1; k < g_iCells; ++k)
            {
                piHashOffsets[k] = piHashOffsets[k - 1] + piHashCount[k - 1];
            }

            try
            {
                for (i = 0; i < iNrTrianglesIn * 3; ++i)
                {
                    int index = piTriList_in_and_out[i];
                    Vector3 vP = GetPosition(ref pContext, index);
                    float fVal = iChannel == 0 ? vP.X : (iChannel == 1) ? vP.Y : vP.Z;
                    int iCell = FindGridCell(fMin, fMax, fVal);

                    piHashTable[piHashOffsets[iCell] + piHashCount2[iCell]] = i;
                    piHashCount2[iCell]++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            piHashCount2 = null;

            iMaxCount = piHashCount[0];
            for(k = 1; k < g_iCells; ++k)
            {
                if(iMaxCount < piHashCount[k])
                {
                    iMaxCount = piHashCount[k];
                }
            }

            pTmpVer = new STmpVert[iMaxCount];

            try
            {
                for (k = 0; k < g_iCells; ++k)
                {

                    int entries = piHashCount[k];
                    if (entries < 2) continue;

                    for (e = 0; e < entries; e++)
                    {
                        pTmpVer[e].vert = new float[3];
                        int iv = piHashTable[piHashOffsets[k] + e];
                        Vector3 vP = GetPosition(ref pContext, piTriList_in_and_out[iv]);
                        pTmpVer[e].vert[0] = vP.X; pTmpVer[e].vert[1] = vP.Y; pTmpVer[e].vert[2] = vP.Z;
                        pTmpVer[e].index = iv;
                    }

                    //merge verts fast here
                    MergeVertsFast(piTriList_in_and_out, ref pTmpVer, ref pContext, 0, entries - 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            pTmpVer = null;
        }

        static void MergeVertsFast(int[] piTriList_in_and_out, ref STmpVert[] pTmpVert, ref SMikkTSpaceContext context, int iL_in, int iR_in)
        {
            int c = 0, l = 0, channel = 0;
            float[] fvMin = new float[3], fvMax = new float[3];
            float dx = 0, dy = 0, dz = 0, fSep = 0;

            for(c = 0; c < 3; c++)
            {
                fvMin[c] = pTmpVert[iL_in].vert[c]; fvMax[c] = fvMin[c];
            }
            for(l = (iL_in + 1); l <= iR_in; l++)
            {
                for(c = 0; c < 3; c++)
                {
                    if (fvMin[c] > pTmpVert[l].vert[c]) fvMin[c] = pTmpVert[l].vert[c];
                    else if (fvMax[c] < pTmpVert[l].vert[c]) fvMax[c] = pTmpVert[l].vert[c];
                }
            }

            dx = fvMax[0] - fvMin[0];
            dy = fvMax[1] - fvMin[1];
            dz = fvMax[2] - fvMin[2];

            channel = 0;
            if (dy > dx && dy > dz) channel = 1;
            else if (dz > dx) channel = 2;

            fSep = 0.5f * (fvMax[channel] + fvMin[channel]);

            if(fSep >= fvMax[channel] || fSep <= fvMin[channel])
            {
                for(l = iL_in; l <= iR_in; l++)
                {
                    int i = pTmpVert[l].index;
                    int index = piTriList_in_and_out[i];
                    Vector3 vP = GetPosition(ref context, index);
                    Vector3 vN = GetNormal(ref context, index);
                    Vector2 vT = GetTexCoord(ref context, index);

                    bool bNotFound = true;
                    int l2 = iL_in, i2rec = -1;

                    while(l2 < l && bNotFound)
                    {
                        int i2 = pTmpVert[l2].index;
                        int index2 = piTriList_in_and_out[i2];
                        Vector3 vP2 = GetPosition(ref context, index2);
                        Vector3 vN2 = GetNormal(ref context, index2);
                        Vector2 vT2 = GetTexCoord(ref context, index2);
                        i2rec = i2;

                        if(vP.Equals(vP2) && vN.Equals(vN2) && vT.Equals(vT2))
                        {
                            bNotFound = false;
                        }
                        else
                        {
                            l2++;
                        }
                    }

                    if(!bNotFound)
                    {
                        piTriList_in_and_out[i] = piTriList_in_and_out[i2rec];
                    }
                }
            }
            else
            {
                int iL = iL_in, iR = iR = iR_in;

                while(iL < iR)
                {
                    bool bReadyLeftSwap = false, bReadyRightSwap = false;
                    while((!bReadyLeftSwap) && iL < iR)
                    {
                        bReadyLeftSwap = !(pTmpVert[iL].vert[channel] < fSep);
                        if (!bReadyLeftSwap) ++iL;
                    }
                    while((!bReadyRightSwap) && iL < iR)
                    {
                        bReadyRightSwap = pTmpVert[iR].vert[channel] < fSep;
                        if (!bReadyRightSwap) --iR;
                    }

                    if(bReadyLeftSwap && bReadyRightSwap)
                    {
                        STmpVert sTmp = pTmpVert[iL];
                        pTmpVert[iL] = pTmpVert[iR];
                        pTmpVert[iR] = sTmp;
                        ++iL; --iR;
                    }
                }

                if(iL == iR)
                {
                    bool bReadyRightSwap = pTmpVert[iR].vert[channel] < fSep;
                    if (bReadyRightSwap) ++iL;
                    else --iR;
                }

                if(iL_in < iR)
                {
                    MergeVertsFast(piTriList_in_and_out, ref pTmpVert, ref context, iL_in, iR);
                }
                if(iL < iR_in)
                {
                    MergeVertsFast(piTriList_in_and_out, ref pTmpVert, ref context, iL, iR_in);
                }
            }
        }

        static int GenerateInitialVerticesIndexList(ref STriInfo[] pTriInfos, int[] piTriList_out, ref SMikkTSpaceContext context, int iNrTrianglesIn)
        {
            int iTSpacesOffs = 0, f = 0, t = 0;
            int iDstTriIndex = 0;

            for(f = 0; f < context.m_pInterface.m_getNumFaces(ref context); f++)
            {
                int verts = context.m_pInterface.m_getNumVerticesOfFace(ref context, f);
                if (verts != 3 && verts != 4) continue;

                pTriInfos[iDstTriIndex].iOrgFaceNumber = f;
                pTriInfos[iDstTriIndex].iTSpacesOffs = iTSpacesOffs;

                if(verts == 3)
                {
                    if(pTriInfos[iDstTriIndex].vert_num == null)
                    {
                        pTriInfos[iDstTriIndex].vert_num = new int[3];
                    }

                    int[] pVerts = pTriInfos[iDstTriIndex].vert_num;
                    pVerts[0] = 0; pVerts[1] = 1; pVerts[2] = 2;
                    piTriList_out[iDstTriIndex * 3] = MakeIndex(f, 0);
                    piTriList_out[iDstTriIndex * 3 + 1] = MakeIndex(f, 1);
                    piTriList_out[iDstTriIndex * 3 + 2] = MakeIndex(f, 2);
                    ++iDstTriIndex;
                }
                else
                {
                    continue;
                }

                iTSpacesOffs += verts;
            }

            for(t = 0; t < iNrTrianglesIn; t++)
            {
                pTriInfos[t].iFlag = 0;
            }

            return iTSpacesOffs;
        }


        static Vector3 GetPosition(ref SMikkTSpaceContext context, int index)
        {
            int iF, iI;
            Vector3 res = new Vector3();
            IndexToData(out iF, out iI, index);
            res = context.m_pInterface.m_getPosition(ref context, iF, iI);
            return res;
        }

        static Vector3 GetNormal(ref SMikkTSpaceContext context, int index)
        {
            int iF, iI;
            Vector3 res = new Vector3();
            IndexToData(out iF, out iI, index);
            res = context.m_pInterface.m_getNormal(ref context, iF, iI);
            return res;
        }

        static Vector2 GetTexCoord(ref SMikkTSpaceContext context, int index)
        {
            int iF, iI;
            Vector2 res = new Vector2();
            IndexToData(out iF, out iI, index);
            res = context.m_pInterface.m_getTexCoord(ref context, iF, iI);
            return res;
        }

 /// <summary>
 /// /////////////////////////////////////////////////////////////////////////////////////////
 /// </summary>

        struct SEdge
        {
            public int i0;
            public int i1;
            public int f;
        }

        static float CalcTexArea(ref SMikkTSpaceContext context, int[] indices, int offset)
        {
            Vector2 t1 = GetTexCoord(ref context, indices[offset + 0]);
            Vector2 t2 = GetTexCoord(ref context, indices[offset + 1]);
            Vector2 t3 = GetTexCoord(ref context, indices[offset + 2]);

            Vector2 t2x = t2 - t1;
            Vector2 t3x = t3 - t1;

            float fSignedAreaSTx2 = t2x.X * t3x.Y - t2x.Y * t3x.X;

            return fSignedAreaSTx2 < 0 ? (-fSignedAreaSTx2) : fSignedAreaSTx2;
        }

        static void InitTriInfo(ref STriInfo[] pTriInfos, int[] pTriListIn, ref SMikkTSpaceContext context, int iNrTrianglesIn)
        {
            int f = 0, i = 0;

            for(f = 0; f < iNrTrianglesIn; ++f)
            {
                pTriInfos[f].faceNeighbors = new int[3];
                pTriInfos[f].AssignedGroup = new SGroup[3];

                for (i = 0; i < 3; ++i)
                {
                    pTriInfos[f].faceNeighbors[i] = -1;
                    pTriInfos[f].iFlag |= GROUP_WITH_ANY;
                }
            }

            for(f = 0; f < iNrTrianglesIn; ++f)
            {
                Vector3 v1 = GetPosition(ref context, pTriListIn[f * 3]);
                Vector3 v2 = GetPosition(ref context, pTriListIn[f * 3 + 1]);
                Vector3 v3 = GetPosition(ref context, pTriListIn[f * 3 + 2]);
                Vector2 t1 = GetTexCoord(ref context, pTriListIn[f * 3]);
                Vector2 t2 = GetTexCoord(ref context, pTriListIn[f * 3 + 1]);
                Vector2 t3 = GetTexCoord(ref context, pTriListIn[f * 3 + 2]);

                Vector2 t2x = t2 - t1;
                Vector2 t3x = t3 - t1;

                Vector3 d1 = v2 - v1;
                Vector3 d2 = v3 - v1;

                float signedAreasSTx2 = t2x.X * t3x.Y - t2x.Y * t3x.X;

                Vector3 vOs = d1 * t3x.Y - d2 * t2x.Y;
                Vector3 vOt = d1 * -t3x.X - d2 * t2x.X;

                pTriInfos[f].iFlag |= (signedAreasSTx2 > 0 ? ORIENT_PRESERVING : 0);

                if(NotZero(signedAreasSTx2))
                {
                    float fAbsArea = Math.Abs(signedAreasSTx2);
                    float fLenOs = vOs.Length;
                    float fLenOt = vOt.Length;
                    float fS = ((pTriInfos[f].iFlag & ORIENT_PRESERVING) == 0) ? -1.0f : 1.0f;
                    if (NotZero(fLenOs)) pTriInfos[f].vOs = fS / fLenOs * vOs;
                    if (NotZero(fLenOt)) pTriInfos[f].vOt = fS / fLenOt * vOt;

                    pTriInfos[f].fMagS = fLenOs / fAbsArea;
                    pTriInfos[f].fMagT = fLenOt / fAbsArea;

                    if(NotZero(pTriInfos[f].fMagS) && NotZero(pTriInfos[f].fMagT))
                    {
                        pTriInfos[f].iFlag &= (~GROUP_WITH_ANY);
                    }
                }
            }
            {
                SEdge[] pEdges = new SEdge[iNrTrianglesIn * 3];
                //build neighbors fast
                BuildNeighborsFast(ref pTriInfos, ref pEdges, pTriListIn, iNrTrianglesIn);
            }
        }

        static int Build4RuleGroups(ref STriInfo[] pTriInfos, ref SGroup[] pGroups, int[] pGroupTrianglesBuffer, int[] piTriListIn, int iNrTrianglesIn)
        {
            int iNrMaxGroups = iNrTrianglesIn * 3;
            int iNrActiveGroups = 0;
            int iOffset = 0, f = 0, i = 0;
            for(f = 0; f < iNrTrianglesIn; ++f)
            {
                for(i = 0; i < 3; ++i)
                {
                    if((pTriInfos[f].iFlag & GROUP_WITH_ANY) == 0 && pTriInfos[f].AssignedGroup[i] == null)
                    {
                        bool bOrPre;
                        int neigh_indexL, neight_indexR;
                        int vert_index = piTriListIn[f * 3 + i];
                        pTriInfos[f].AssignedGroup[i] = pGroups[iNrActiveGroups];
                        pTriInfos[f].AssignedGroup[i].iVertex = vert_index;
                        pTriInfos[f].AssignedGroup[i].bOrientPreservering = (pTriInfos[f].iFlag & ORIENT_PRESERVING) != 0;
                        pTriInfos[f].AssignedGroup[i].iNrFaces = 0;
                        pTriInfos[f].AssignedGroup[i].faceOffset = iOffset;
                        ++iNrActiveGroups;

                        //add tri to group...
                        AddTriToGroup(ref pTriInfos[f].AssignedGroup[i], pGroupTrianglesBuffer, f);
                        bOrPre = (pTriInfos[f].iFlag & ORIENT_PRESERVING) != 0;
                        neigh_indexL = pTriInfos[f].faceNeighbors[i];
                        neight_indexR = pTriInfos[f].faceNeighbors[i > 0 ? (i - 1) : 2];
                        if(neigh_indexL >= 0)
                        {

                            //assignrecur
                            AssignRecur(piTriListIn, pGroupTrianglesBuffer, ref pTriInfos, neigh_indexL, ref pTriInfos[f].AssignedGroup[i]);
                        }
                        if(neight_indexR >= 0)
                        {
                            //assignrecur
                            AssignRecur(piTriListIn, pGroupTrianglesBuffer, ref pTriInfos, neight_indexR, ref pTriInfos[f].AssignedGroup[i]);
                        }

                        iOffset += pTriInfos[f].AssignedGroup[i].iNrFaces;
                    }
                }
            }

            return iNrActiveGroups;
        }

        static void AddTriToGroup(ref SGroup group, int[] pGroupTriangleBuffer, int iTriIndex)
        {
            pGroupTriangleBuffer[group.iNrFaces + group.faceOffset] = iTriIndex;
            group.iNrFaces++;
        }

        static void AssignRecur(int[] piTriListIn, int[] pGroupTriangleBuffer, ref STriInfo[] psTriInfos, int iMyTriIndex, ref SGroup pGroup)
        {
            int i = -1;
            int iVertRep = pGroup.iVertex;
            if (piTriListIn[3 * iMyTriIndex] == iVertRep) i = 0;
            else if (piTriListIn[3 * iMyTriIndex + 1] == iVertRep) i = 1;
            else if (piTriListIn[3 * iMyTriIndex + 2] == iVertRep) i = 2;

            if (psTriInfos[iMyTriIndex].AssignedGroup[i] == pGroup) return;
            else if (psTriInfos[iMyTriIndex].AssignedGroup[i] != null) return;
            if((psTriInfos[iMyTriIndex].iFlag & GROUP_WITH_ANY) != 0)
            {
                if(psTriInfos[iMyTriIndex].AssignedGroup[0] == null
                    && psTriInfos[iMyTriIndex].AssignedGroup[1] == null
                    && psTriInfos[iMyTriIndex].AssignedGroup[2] == null)
                {
                    psTriInfos[iMyTriIndex].iFlag &= (~ORIENT_PRESERVING);
                    psTriInfos[iMyTriIndex].iFlag |= (pGroup.bOrientPreservering ? ORIENT_PRESERVING : 0);
                }
            }
            {
                bool bOrient = (psTriInfos[iMyTriIndex].iFlag & ORIENT_PRESERVING) != 0;
                if (bOrient != pGroup.bOrientPreservering) return;
            }

            AddTriToGroup(ref pGroup, pGroupTriangleBuffer, iMyTriIndex);
            psTriInfos[iMyTriIndex].AssignedGroup[i] = pGroup;

            {
                int neigh_indexL = psTriInfos[iMyTriIndex].faceNeighbors[i];
                int neigh_indexR = psTriInfos[iMyTriIndex].faceNeighbors[i > 0 ? (i - 1) : 2];
                if(neigh_indexL >= 0)
                {
                    AssignRecur(piTriListIn, pGroupTriangleBuffer, ref psTriInfos, neigh_indexL, ref pGroup);
                }
                if(neigh_indexR >= 0)
                {
                    AssignRecur(piTriListIn, pGroupTriangleBuffer, ref psTriInfos, neigh_indexR, ref pGroup);
                }
            }
        }

        static bool GenerateTSpaces(ref STSpace[] psTspace, ref STriInfo[] pTriInfos, SGroup[] pGroups, int iNrActiveGroups, int[] piTriListIn, int[] pGroupTriangleBuffer, float fThresCos, ref SMikkTSpaceContext context)
        {
            STSpace[] pSubGroupTSpace = null;
            SSubGroup[] pUniSubGroups = null;
            int[] pTmpMembers = null;
            int iMaxNrFaces = 0, iUniqueTSpaces = 0, g = 0, i = 0;
            for(g = 0; g < iNrActiveGroups; ++g)
            {
                if(iMaxNrFaces < pGroups[g].iNrFaces)
                {
                    iMaxNrFaces = pGroups[g].iNrFaces;
                }
            }

            if (iMaxNrFaces == 0) return true;

            pSubGroupTSpace = new STSpace[iMaxNrFaces];
            pUniSubGroups = new SSubGroup[iMaxNrFaces];
            pTmpMembers = new int[iMaxNrFaces];

            iUniqueTSpaces = 0;
            for(g = 0; g < iNrActiveGroups; ++g)
            {
                int iUniqueSubGroups = 0;

                for(i = 0; i < pGroups[g].iNrFaces; ++i)
                {
                    int f = pGroupTriangleBuffer[pGroups[g].faceOffset];
                    int index = -1, iVertIndex = -1, iOF_1 = -1, iMembers = 0, j = 0, l = 0;
                    SSubGroup tmp_group;
                    bool bFound = false;
                    Vector3 n = Vector3.Zero, vOs = Vector3.Zero, vOt = Vector3.Zero;
                    if (pTriInfos[f].AssignedGroup[0] == pGroups[g]) index = 0;
                    else if (pTriInfos[f].AssignedGroup[1] == pGroups[g]) index = 1;
                    else if (pTriInfos[f].AssignedGroup[2] == pGroups[g]) index = 2;

                    iVertIndex = piTriListIn[f * 3 + index];

                    n = GetNormal(ref context, iVertIndex);

                    vOs = pTriInfos[f].vOs - (Vector3.Dot(n, pTriInfos[f].vOs) * n);
                    vOt = pTriInfos[f].vOt - (Vector3.Dot(n, pTriInfos[f].vOt) * n);
                    if (VNotZero(ref vOs)) vOs.Normalize();
                    if (VNotZero(ref vOt)) vOt.Normalize();

                    iOF_1 = pTriInfos[f].iOrgFaceNumber;

                    iMembers = 0;
                    for(j = 0; j < pGroups[g].iNrFaces; ++j)
                    {
                        int t = pGroupTriangleBuffer[pGroups[g].faceOffset + j];
                        int iOf_2 = pTriInfos[t].iOrgFaceNumber;

                        Vector3 vOs2 = (pTriInfos[t].vOs - (Vector3.Dot(n, pTriInfos[t].vOs) * n));
                        Vector3 vOt2 = (pTriInfos[t].vOt - (Vector3.Dot(n, pTriInfos[t].vOt) * n));

                        if (VNotZero(ref vOs2)) vOs2.Normalize();
                        if (VNotZero(ref vOt2)) vOt2.Normalize();

                        {
                            bool bAny = ((pTriInfos[f].iFlag | pTriInfos[t].iFlag) & GROUP_WITH_ANY) != 0;
                            bool bSameOrgFace = iOF_1 == iOf_2;

                            float fCosS = Vector3.Dot(vOs, vOs2);
                            float fCosT = Vector3.Dot(vOt, vOt2);

                            if(bAny || bSameOrgFace || (fCosS > fThresCos && fCosT > fThresCos))
                            {
                                pTmpMembers[iMembers++] = t;
                            }
                        }
                    }

                    tmp_group.iNrFaces = iMembers;
                    tmp_group.triMembers = new List<int>(pTmpMembers);
                    if(iMembers > 1)
                    {
                        tmp_group.triMembers.Sort();
                    }

                    bFound = false;
                    l = 0;
                    while(l < iUniqueSubGroups && !bFound)
                    {
                        bFound = CompareSubGroups(ref tmp_group, ref pUniSubGroups[l]);
                        if (!bFound) ++l;
                    }

                    if(!bFound)
                    {
 
                        pUniSubGroups[iUniqueSubGroups].iNrFaces = iMembers;
                        pUniSubGroups[iUniqueSubGroups].triMembers = new List<int>(tmp_group.triMembers);
                        pSubGroupTSpace[iUniqueSubGroups] = EvalTspace(tmp_group.triMembers, iMembers, piTriListIn, ref pTriInfos, ref context, pGroups[g].iVertex);
                        iUniqueSubGroups++;
                    }


                    {
                        int iOffs = pTriInfos[f].iTSpacesOffs;
                        int iVert = pTriInfos[f].vert_num[index];
                        if(psTspace[iOffs + iVert].iCounter == 1)
                        {
                            psTspace[iOffs + iVert] = AvgTSpace(ref psTspace[iOffs + iVert], ref pSubGroupTSpace[l]);
                            psTspace[iOffs + iVert].iCounter = 2;
                            psTspace[iOffs + iVert].bOrient = pGroups[g].bOrientPreservering;
                        }
                        else
                        {
                            psTspace[iOffs + iVert] = pSubGroupTSpace[l];
                            psTspace[iOffs + iVert].iCounter = 1;
                            psTspace[iOffs + iVert].bOrient = pGroups[g].bOrientPreservering;
                        }
                    }
                }

                iUniqueTSpaces += iUniqueSubGroups;
            }

            return true;
        }

        static STSpace EvalTspace(List<int> face_indices, int iFaces, int[] piTriListIn, ref STriInfo[] pTriInfos, ref SMikkTSpaceContext context, int iVertexRepresentitive)
        {
            STSpace res = new STSpace();
            float fAngleSum = 0;
            int face = 0;
            res.vOs = Vector3.Zero;
            res.vOt = Vector3.Zero;
            res.fMagS = 0; res.fMagT = 0;

            for(face = 0; face < iFaces; face++)
            {
                int f = face_indices[face];

                if((pTriInfos[f].iFlag & GROUP_WITH_ANY) == 0)
                {
                    Vector3 n, vOs, vOt, p0, p1, p2, v1, v2;
                    float fCos, fAngle, fMagS, fMagT;
                    int i = -1, index = -1, i0 = -1, i1 = -1, i2 = -1;
                    if (piTriListIn[3 * f] == iVertexRepresentitive) i = 0;
                    else if (piTriListIn[3 * f + 1] == iVertexRepresentitive) i = 1;
                    else if (piTriListIn[3 * f + 2] == iVertexRepresentitive) i = 2;

                    index = piTriListIn[3 * f + i];

                    n = GetNormal(ref context, index);
                    vOs = pTriInfos[f].vOs - (Vector3.Dot(n, pTriInfos[f].vOs) * n);
                    vOt = pTriInfos[f].vOt - (Vector3.Dot(n, pTriInfos[f].vOt) * n);
                    if (VNotZero(ref vOs)) vOs.Normalize();
                    if (VNotZero(ref vOt)) vOt.Normalize();

                    i2 = piTriListIn[3 * f + (i < 2 ? (i + 1) : 0)];
                    i1 = piTriListIn[3 * f + i];
                    i0 = piTriListIn[3 * f + (i > 0 ? (i - 1) : 2)];

                    p0 = GetPosition(ref context, i0);
                    p1 = GetPosition(ref context, i1);
                    p2 = GetPosition(ref context, i2);

                    v1 = p0 - p1;
                    v2 = p2 - p1;

                    v1 = v1 - Vector3.Dot(n, v1) * n;
                    v2 = v2 - Vector3.Dot(n, v2) * n;
                    if (VNotZero(ref v1)) v1.Normalize();
                    if (VNotZero(ref v2)) v2.Normalize();

                    fCos = Vector3.Dot(v1, v2); fCos = fCos > 1 ? 1 : (fCos < -1 ? -1 : fCos);
                    fAngle = (float)Math.Acos(fCos);
                    fMagS = pTriInfos[f].fMagS;
                    fMagT = pTriInfos[f].fMagT;

                    res.vOs = res.vOs + vOs * fAngle;
                    res.vOt = res.vOt + vOt * fAngle;
                    res.fMagS += (fAngle * fMagS);
                    res.fMagT += (fAngle * fMagT);

                    fAngleSum += fAngle;
                }
            }

            if (VNotZero(ref res.vOs)) res.vOs.Normalize();
            if (VNotZero(ref res.vOt)) res.vOt.Normalize();

            if (fAngleSum > 0)
            {
                res.fMagS /= fAngleSum;
                res.fMagT /= fAngleSum;
            }

            return res;
        }

        static bool CompareSubGroups(ref SSubGroup pg1, ref SSubGroup pg2)
        {
            bool bStillSame = true;
            int i = 0;
            if (pg1.iNrFaces != pg2.iNrFaces) return false;
            while(i < pg1.iNrFaces && bStillSame)
            {
                bStillSame = pg1.triMembers[i] == pg2.triMembers[i];
                if (bStillSame) ++i;
            }
            return bStillSame;
        }

        static void BuildNeighborsFast(ref STriInfo[] pTriInfos, ref SEdge[] pEdges, int[] piTriListIn, int iNrTrianglesIn)
        {
            int iEntries = 0, f = 0, i = 0;
            for(f = 0; f < iNrTrianglesIn; ++f)
            {
                for(i = 0; i < 3; ++i)
                {
                    int i0 = piTriListIn[f * 3 + i];
                    int i1 = piTriListIn[f * 3 + (i < 2 ? (i + 1) : 0)];
                    pEdges[f * 3 + i].i0 = i0 < i1 ? i0 : i1;
                    pEdges[f * 3 + i].i1 = !(i0 < i1) ? i0 : i1;
                    pEdges[f * 3 + i].f = f;
                }
            }

            Array.Sort<SEdge>(pEdges, (SEdge e1, SEdge e2) =>
            {
                if (e1.i0 != e2.i0)
                {
                    return e1.i0 - e2.i0;
                }
                else if(e1.i1 != e2.i1)
                {
                    return e1.i1 - e2.i1;
                }
                else
                {
                    return e1.f - e2.f;
                }
            });

            for(i = 0; i < iEntries; ++i)
            {
                int i0 = pEdges[i].i0;
                int i1 = pEdges[i].i1;
                int ff = pEdges[i].f;

                bool Unassigned_A;

                int i0_A, i1_A;
                int edgenum_A = 0, edgenum_B = 0;
                //get edge
                GetEdge(out i0_A, out i1_A, out edgenum_A, piTriListIn, f * 3, i0, i1);
                Unassigned_A = pTriInfos[ff].faceNeighbors[edgenum_A] == -1;

                if(Unassigned_A)
                {
                    int j = i + 1, t;
                    bool bNotFound = true;

                    while(j < iEntries && i0 == pEdges[j].i0 && i1 == pEdges[j].i1 && bNotFound)
                    {
                        bool Unassigned_B;
                        int i0_B, i1_B;
                        t = pEdges[j].f;

                        //get edge
                        GetEdge(out i1_B, out i0_B, out edgenum_B, piTriListIn, t * 3, pEdges[j].i0, pEdges[j].i1);

                        Unassigned_B = pTriInfos[t].faceNeighbors[edgenum_B] == -1;
                        if(i0_A == i0_B && i1_A == i1_B && Unassigned_B)
                        {
                            bNotFound = false;
                        }
                        else
                        {
                            j++;
                        }
                    }

                    if(!bNotFound)
                    {
                        int tt = pEdges[j].f;
                        pTriInfos[ff].faceNeighbors[edgenum_A] = tt;
                        pTriInfos[tt].faceNeighbors[edgenum_B] = ff;
                    }
                }
            }
        }

        static void GetEdge(out int i0_out, out int i1_out, out int edgeNum, int[] indices, int offset, int i0_in, int i1_in)
        {
            edgeNum = -1;
            if(indices[offset] == i0_in || indices[offset] == i1_in)
            {
                if(indices[offset + 1] == i0_in || indices[offset + 1] == i1_in)
                {
                    edgeNum = 0;
                    i0_out = indices[offset];
                    i1_out = indices[offset + 1];
                }
                else
                {
                    edgeNum = 2;
                    i0_out = indices[offset + 2];
                    i1_out = indices[offset];
                }
            }
            else
            {
                edgeNum = 1;
                i0_out = indices[offset + 1];
                i1_out = indices[offset + 2];
            }
        }
    }
}
