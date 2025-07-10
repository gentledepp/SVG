namespace Svg.Editor.Sample.Forms
{
    public partial class EditorPage
    {
        public EditorPage()
        {
            InitializeComponent();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            var viewModel = BindingContext as MainViewModel;
            viewModel?.OnDisappearing();
        }
    }
}
