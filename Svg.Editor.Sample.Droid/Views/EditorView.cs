using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using MvvmCross.Droid.Views;
using Svg.Editor.Interfaces;
using Svg.Editor.Sample.Core.ViewModels;
using Svg.Editor.Services;
using Svg.Editor.Views.Droid;
using Path = System.IO.Path;

namespace Svg.Editor.Sample.Droid.Views
{
    [Activity(Label = "Edit SVG", Exported = true)]
    public class EditorView : MvxActivity
    {
        private AndroidSvgCanvasEditorView _padView;
        private Dictionary<string, int> _iconCache = new Dictionary<string, int>();
	    private Lazy<IToolbarIconSizeProvider> _toolbarIconSizeProvider = new Lazy<IToolbarIconSizeProvider>(SvgEngine.TryResolve<IToolbarIconSizeProvider>);

		protected override void OnCreate(Bundle bundle)
        {
            SetupIconCache();

            base.OnCreate(bundle);

            SetContentView(Resource.Layout.EditorView);
            _padView = FindViewById<AndroidSvgCanvasEditorView>(Resource.Id.pad);

            _padView.DrawingCanvas = ViewModel.Canvas;
        }

        private void SetupIconCache()
        {
            var t = typeof(Resource.Drawable);
            foreach (var constant in t.GetFields().Where(@fld => @fld.IsLiteral))
            {
                var rawValue = constant.GetRawConstantValue();
                if (!(rawValue is int))
                    continue;

                _iconCache.Add(constant.Name, (int)rawValue);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var shownActions = 3;

            foreach (var commands in ViewModel.Canvas.ToolCommands)
            {
                var cmds = commands.Where(c => c.CanExecute(null)).ToArray();
                if (cmds.Length == 0)
                    continue;

                if (cmds.Length == 1)
                {
                    var cmd = cmds.Single();
                    var mi = menu.Add(cmd.GetHashCode(), cmd.GetHashCode(), 1, cmd.Name);
                    SetIcon(mi, cmd.IconName);

                    if (shownActions > 0)
                        mi.SetShowAsAction(ShowAsAction.IfRoom);
                    else
                        mi.SetShowAsAction(ShowAsAction.Never);
                }
                else
                {
                    var c = cmds.First();

                    var m = menu.AddSubMenu(c.Tool.GetHashCode(), c.Tool.GetHashCode(), 1, c.GroupName);
                    SetIcon(m.Item, c.GroupIconName);

                    foreach (var cmd in cmds)
                    {
                        var mi = m.Add(cmd.GetHashCode(), cmd.GetHashCode(), 1, cmd.Name);
                        SetIcon(mi, cmd.IconName);
                    }

                    if (shownActions > 0)
                        m.Item.SetShowAsAction(ShowAsAction.IfRoom);
                    else
                        m.Item.SetShowAsAction(ShowAsAction.Never);

                }

                shownActions--;
            }

            return true;
        }

        private void SetIcon(IMenuItem item, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            var n = Path.GetFileNameWithoutExtension(name);
	        var iconDimension = _toolbarIconSizeProvider.Value?.GetSize();


			if (string.IsNullOrWhiteSpace(n))
                return;

            int value;
            if (_iconCache.TryGetValue(n, out value))
            {
                item.SetIcon(value);
            }
            else if (File.Exists(name))
            {
	            item.SetIcon(Drawable.CreateFromPath(name));
            }
            else
            {
	            var path = SvgEngine.Resolve<IImageSourceProvider>().GetImage(name, iconDimension);
	            item.SetIcon(Drawable.CreateFromPath(path));
            }
        }

        public new EditorViewModel ViewModel
        {
            get { return (EditorViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var cmd =
                ViewModel.Canvas.ToolCommands.SelectMany(c => c).FirstOrDefault(c => c.GetHashCode() == item.ItemId);
            if (cmd != null)
            {
                cmd.Execute(_padView);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

	    protected override void OnPause()
	    {
		    base.OnPause();

			ViewModel.SaveToolPropertiesCommand.Execute();
	    }
    }
}
