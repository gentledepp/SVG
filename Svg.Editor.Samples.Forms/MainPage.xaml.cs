using System;

namespace Svg.Editor.Samples.Forms
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
