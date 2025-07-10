using System;

namespace Svg.Editor.Sample.Forms
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new EditorPage());
        }
    }
}
