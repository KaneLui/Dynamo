﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Resources;
using CefSharp;
using Dynamo.Engine;
using Dynamo.Interfaces;

namespace Dynamo.LibraryUI.Handlers
{
    /// <summary>
    /// Implements resource provider for icons
    /// </summary>
    class IconResourceProvider : ResourceProviderBase
    {
        private const string imagesSuffix = "Images";
        private IPathManager pathManager;

        public IconResourceProvider(IPathManager pathManager) : base(true)
        {
            this.pathManager = pathManager;
        }

        public override Stream GetResource(IRequest request, out string extension)
        {
            extension = "png";
            //Create IconUrl to parse the request.Url to icon name and path.
            var icon = new IconUrl(new Uri(request.Url));
            var path = Path.GetFullPath(icon.Path); //Get full path if it's a relative path.
            var libraryCustomization = LibraryCustomizationServices.GetForAssembly(path, pathManager, true);
            if (libraryCustomization == null)
                return null;

            var assembly = libraryCustomization.ResourceAssembly;
            if (assembly == null)
                return null;
            
            // "Name" can be "Some.Assembly.Name.customization" with multiple dots, 
            // we are interested in removal of the "customization" part and the middle dots.
            var temp = assembly.GetName().Name.Split('.');
            var assemblyName = String.Join("", temp.Take(temp.Length - 1));
            var rm = new ResourceManager(assemblyName + imagesSuffix, assembly);

            using (var image = (Bitmap)rm.GetObject(icon.Name))
            {
                if (image == null) return null;

                var stream = new MemoryStream();
                image.Save(stream, ImageFormat.Png);

                return stream;
            }
        }
    }
}