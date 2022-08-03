using Svg.Editor.Samples.Forms;

namespace Svg.Editor.Samples.Forms
{
    public partial class EditorPage : ContentPage
    {
        public EditorPage()
        {
            InitializeComponent();
            BindingContext = new MainViewModel(this);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            var viewModel = BindingContext as MainViewModel;
            viewModel?.OnDisappearing();
        }
    }
}
