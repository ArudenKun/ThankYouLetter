using System;
using ThankYouLetter.ViewModels;

namespace ThankYouLetter.Views;

public interface IView<out TViewModel>
    where TViewModel : ViewModel;
