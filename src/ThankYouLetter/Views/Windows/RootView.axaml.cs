using ThankYouLetter.Dependency;
using ThankYouLetter.ViewModels.Windows;

namespace ThankYouLetter.Views.Windows;

[Singleton]
public partial class RootView : SukiWindow<RootViewModel>
{
    public RootView()
    {
        InitializeComponent();
    }
}
