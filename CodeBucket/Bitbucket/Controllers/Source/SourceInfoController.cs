using System;
using MonoTouch.UIKit;
using CodeBucket.Bitbucket.Controllers;
using MonoTouch.Foundation;
using CodeBucket.Bitbucket.Controllers;
using BitbucketSharp;

namespace CodeBucket.Bitbucket.Controllers.Source
{
    public class SourceInfoController : FileSourceController
    {
        protected string _user, _slug, _branch, _path;
        
        public SourceInfoController(string user, string slug, string branch, string path)
        {
            _user = user;
            _slug = slug;
            _branch = branch;
            _path = path;
            
            //Create the filename
            var fileName = System.IO.Path.GetFileName(path);
            if (fileName == null)
                fileName = path.Substring(path.LastIndexOf('/') + 1);

            //Create the temp file path
            Title = fileName;
        }

        protected override void Request()
        {
            //There is a bug in the Bitbucket server that says everything returned is text. Content Type: text/plain
            //Attempt to load this the normal way... If we fail then we'll fall back. If that fails then just display an error.
            try 
            {
                //If this is successful there will be no exception. Just exit out!
                var d = Application.Client.Users[_user].Repositories[_slug].Branches[_branch].Source.GetFile(_path);

                //If the encoding is a base64 then assume it is a binary
                if (d.Encoding != null && d.Encoding.Equals("base64"))
                {
                    //Save the data to the disk
                    var decodedData = System.Convert.FromBase64String(d.Data);
                    var path = System.IO.Path.Combine(TempDir, System.IO.Path.GetFileName(_path));
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(TempDir, path), decodedData);
                    LoadFile(path);
                }
                //If there is no encoding, or it's not base64 then don't worry about it. It's most likely text.
                else
                {
                    var data = System.Security.SecurityElement.Escape(d.Data);
                    LoadRawData(data);
                }

                //Nothing else to do!
                return;
            }
            catch (InternalServerException ex)
            {
                Console.WriteLine("Could not grab file the bitbucket way: " + ex.Message);
            }
            
            //Attempt to load hte file the raw way.
            var file = DownloadFile(_user, _slug, _branch, _path);
            LoadFile(file);
        }
    }
}

