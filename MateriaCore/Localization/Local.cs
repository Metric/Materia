using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MateriaCore.Localization
{
    public class Local : MarkupExtension
    {
        string Id { get; set; }
        IAssetLoader Assets { get; set; }
        Dictionary<string, Dictionary<string, string>> Language { get; set; }

        public Local()
        {
            try
            {
                Assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                var assy = Assembly.GetExecutingAssembly();
                EmbeddedFileProvider provider = new EmbeddedFileProvider(assy);
                Language = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(new StreamReader(provider.GetFileInfo("Localization/localization.json").CreateReadStream()).ReadToEnd());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + " | " + e.StackTrace);
            }
        }

        public Local(string id) : this()
        {
            Id = id;
        }

        public string Get(string id)
        {
            string v = "";
            try
            {
                var currentCulture = CultureInfo.CurrentCulture;
                if (Language.ContainsKey(currentCulture.Name))
                {

                    Language[currentCulture.Name].TryGetValue(id, out v);
                }
                else
                {
                    Language["default"].TryGetValue(id, out v);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + " | " + e.StackTrace);
            }
            return v;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            string v = "";
            try
            {
                if(Language == null)
                {
                    return "";
                }

                var currentCulture = CultureInfo.CurrentCulture;
                if (Language.ContainsKey(currentCulture.Name))
                {

                    Language[currentCulture.Name].TryGetValue(Id, out v);
                }
                else
                {
                    Language["default"].TryGetValue(Id, out v);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + " | " + e.StackTrace);
            }

            return v;
        }
    }
}
