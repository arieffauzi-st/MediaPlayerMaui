using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using MauiAppMedia.Services;
using Mopups.Pages;
using Mopups.Services;

namespace MauiAppMedia;

public partial class FullScreenPage : PopupPage
{
    private readonly IDeviceOrientationService deviceOrientationService;
    public CurrentVideoState Video { get; set; }

    public FullScreenPage(CurrentVideoState currentVideo)
    {
        Video = currentVideo;
        InitializeComponent();
        //deviceOrientationService = new DeviceOrientationService();
        //deviceOrientationService.SetDeviceOrientation(displayOrientation: DisplayOrientation.Landscape);
        deviceOrientationService = new AndroidDeviceOrientationService();
        deviceOrientationService.SetDeviceOrientation(displayOrientation: DisplayOrientation.Landscape);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        mediaElement.Source = Video.VideoUri;
        mediaElement.SeekTo(Video.Position);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        mediaElement.Source = null;
        WeakReferenceMessenger.Default.Send(new NotifyFullScreenClosed(true));
        await MopupService.Instance.PopAsync();
    }

    private void btnChangeAspect_Clicked(object sender, EventArgs e)
    {
        if (mediaElement.Aspect == Aspect.AspectFit)
            MainThread.BeginInvokeOnMainThread(() => mediaElement.Aspect = Aspect.Fill);
        else if (mediaElement.Aspect == Aspect.Fill)
            MainThread.BeginInvokeOnMainThread(() => mediaElement.Aspect = Aspect.Center);
        else if (mediaElement.Aspect == Aspect.Center)
            MainThread.BeginInvokeOnMainThread(() => mediaElement.Aspect = Aspect.AspectFit);
    }

    private void btnChangeOrientation_Clicked(object sender, EventArgs e)
    {
        switch (DeviceDisplay.Current.MainDisplayInfo.Orientation)
        {
            case DisplayOrientation.Landscape:
                deviceOrientationService.SetDeviceOrientation(DisplayOrientation.Portrait);
                break;
            case DisplayOrientation.Portrait:
                deviceOrientationService.SetDeviceOrientation(DisplayOrientation.Landscape);
                break;
        }
    }

}

public class CurrentVideoState
{
    public Uri? VideoUri { get; set; }
    public TimeSpan Position { get; set; }
}

public class NotifyFullScreenClosed : ValueChangedMessage<bool>
{
    public NotifyFullScreenClosed(bool value) : base(value) { }
}