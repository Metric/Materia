namespace VCDiff.Includes
{
    public enum VCDiffResult
    {
        SUCCESS = 0,
        ERRROR = -1,
        EOD = -2
    }

    // The possible values for the Hdr_Indicator field, as described
    // in section 4.1 of the RFC:
    //
    //    "The Hdr_Indicator byte shows if there is any initialization data
    //     required to aid in the reconstruction of data in the Window sections.
    //     This byte MAY have non-zero values for either, both, or neither of
    //     the two bits VCD_DECOMPRESS and VCD_CODETABLE below:
    //
    //         7 6 5 4 3 2 1 0
    //        +-+-+-+-+-+-+-+-+
    //        | | | | | | | | |
    //        +-+-+-+-+-+-+-+-+
    //                     ^ ^
    //                     | |
    //                     | +-- VCD_DECOMPRESS
    //                     +---- VCD_CODETABLE
    //
    //     If bit 0 (VCD_DECOMPRESS) is non-zero, this indicates that a
    //     secondary compressor may have been used to further compress certain
    //     parts of the delta encoding data [...]"
    // [Secondary compressors are not supported by open-vcdiff.]
    //
    //    "If bit 1 (VCD_CODETABLE) is non-zero, this indicates that an
    //     application-defined code table is to be used for decoding the delta
    //     instructions. [...]"
    //
    public enum VCDiffCodeFlags
    {
        VCDDECOMPRESS = 0x01,
        VCDCODETABLE = 0x02
    }

    // The possible values for the Win_Indicator field, as described
    // in section 4.2 of the RFC:
    //
    //    "Win_Indicator:
    //
    //     This byte is a set of bits, as shown:
    //
    //      7 6 5 4 3 2 1 0
    //     +-+-+-+-+-+-+-+-+
    //     | | | | | | | | |
    //     +-+-+-+-+-+-+-+-+
    //                  ^ ^
    //                  | |
    //                  | +-- VCD_SOURCE
    //                  +---- VCD_TARGET
    //
    //     If bit 0 (VCD_SOURCE) is non-zero, this indicates that a
    //     segment of data from the "source" file was used as the
    //     corresponding source window of data to encode the target
    //     window.  The decoder will use this same source data segment to
    //     decode the target window.
    //
    //     If bit 1 (VCD_TARGET) is non-zero, this indicates that a
    //     segment of data from the "target" file was used as the
    //     corresponding source window of data to encode the target
    //     window.  As above, this same source data segment is used to
    //     decode the target window.
    //
    //     The Win_Indicator byte MUST NOT have more than one of the bits
    //     set (non-zero).  It MAY have none of these bits set."
    //
    public enum VCDiffWindowFlags
    {
        VCDSOURCE = 0x01,
        VCDTARGET = 0x02,
        //Google Specific Flag
        VCDCHECKSUM = 0x04
    }

    // The possible values for the Delta_Indicator field, as described
    // in section 4.3 of the RFC:
    //
    //    "Delta_Indicator:
    //     This byte is a set of bits, as shown:
    //
    //      7 6 5 4 3 2 1 0
    //     +-+-+-+-+-+-+-+-+
    //     | | | | | | | | |
    //     +-+-+-+-+-+-+-+-+
    //                ^ ^ ^
    //                | | |
    //                | | +-- VCD_DATACOMP
    //                | +---- VCD_INSTCOMP
    //                +------ VCD_ADDRCOMP
    //
    //          VCD_DATACOMP:   bit value 1.
    //          VCD_INSTCOMP:   bit value 2.
    //          VCD_ADDRCOMP:   bit value 4.
    //
    //     [...] If the bit VCD_DECOMPRESS (Section 4.1) was on, each of these
    //     sections may have been compressed using the specified secondary
    //     compressor.  The bit positions 0 (VCD_DATACOMP), 1
    //     (VCD_INSTCOMP), and 2 (VCD_ADDRCOMP) respectively indicate, if
    //     non-zero, that the corresponding parts are compressed."
    // [Secondary compressors are not supported, so open-vcdiff decoding will fail
    //  if these bits are not all zero.]
    public enum VCDiffCompressFlags
    {
        VCDDATACOMP = 0x01,
        VCDINSTCOMP = 0x02,
        VCDADDRCOMP = 0x04
    }

    // The address modes used for COPY instructions, as defined in
    // section 5.3 of the RFC.
    //
    // The first two modes (0 and 1) are defined as SELF (addressing forward
    // from the beginning of the source window) and HERE (addressing backward
    // from the current position in the source window + previously decoded
    // target data.)
    //
    // After those first two modes, there are a variable number of NEAR modes
    // (which take a recently-used address and add a positive offset to it)
    // and SAME modes (which match a previously-used address using a "hash" of
    // the lowest bits of the address.)  The number of NEAR and SAME modes
    // depends on the defined size of the address cache; since this number is
    // variable, these modes cannot be specified as enum values.
    public enum VCDiffModes
    {
        SELF = 0,
        HERE = 1,
        FIRST = 2,
        MAX = byte.MaxValue + 1
    }

    public enum VCDiffInstructionType
    {
        NOOP = 0,
        ADD = 1,
        RUN = 2,
        COPY = 3,
        LAST = 3,
        ERROR = 4,
        EOD = 5
    }
}
