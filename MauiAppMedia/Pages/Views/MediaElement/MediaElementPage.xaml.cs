using System.ComponentModel;
using CommunityToolkit.Maui.Core.Primitives;
using MauiAppMedia.ViewModels.Views;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Messaging;
using MauiAppMedia.Services;
using Mopups.Services;
using SystemUri = System.Uri;

namespace MauiAppMedia.Pages.Views;

public partial class MediaElementPage : BasePage<MediaElementViewModel>
{
	readonly ILogger logger;
	const string loadOnlineMp4 = "Load Online MP4";
	const string loadHls = "Load HTTP Live Stream (HLS)";
	const string loadLocalResource = "Load Local Resource";
	const string resetSource = "Reset Source to null";
    

    public MediaElementPage(MediaElementViewModel viewModel, ILogger<MediaElementPage> logger) : base(viewModel)
	{
		InitializeComponent();

		this.logger = logger;
#if ANDROID
		btnFullScreen.IsVisible = true;
#elif IOS
		        btnFullScreen.IsVisible = false;
#endif
        
        mediaElement.PropertyChanged += MediaElement_PropertyChanged;
		WeakReferenceMessenger.Default.Register<MediaElementPage, NotifyFullScreenClosed>(this, OnFullScreenClosed);

	}


	void MediaElement_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == MediaElement.DurationProperty.PropertyName)
		{
			logger.LogInformation("Duration: {newDuration}", mediaElement.Duration);
			positionSlider.Maximum = mediaElement.Duration.TotalSeconds;
		}
	}

	void OnMediaOpened(object? sender, EventArgs e) => logger.LogInformation("Media opened.");

	void OnStateChanged(object? sender, MediaStateChangedEventArgs e) =>
		logger.LogInformation("Media State Changed. Old State: {PreviousState}, New State: {NewState}", e.PreviousState, e.NewState);

	void OnMediaFailed(object? sender, MediaFailedEventArgs e) => logger.LogInformation("Media failed. Error: {ErrorMessage}", e.ErrorMessage);

	void OnMediaEnded(object? sender, EventArgs e) => logger.LogInformation("Media ended.");

	private TimeSpan currentPosition;
	void OnPositionChanged(object? sender, MediaPositionChangedEventArgs e)
	{
		logger.LogInformation("Position changed to {position}", e.Position);

		currentPosition = e.Position;

		positionSlider.Value = e.Position.TotalSeconds;
	}

	void OnSeekCompleted(object? sender, EventArgs e) => logger.LogInformation("Seek completed.");

	void OnSpeedMinusClicked(object? sender, EventArgs e)
	{
		if (mediaElement.Speed >= 1)
		{
			mediaElement.Speed -= 1;
		}
	}

	void OnSpeedPlusClicked(object? sender, EventArgs e)
	{
		if (mediaElement.Speed < 10)
		{
			mediaElement.Speed += 1;
		}
	}

	//void OnVolumeMinusClicked(object? sender, EventArgs e)
	//{
	//	if (mediaElement.Volume >= 0)
	//	{
	//		if (mediaElement.Volume < .1)
	//		{
	//			mediaElement.Volume = 0;

	//			return;
	//		}

	//		mediaElement.Volume -= .1;
	//	}
	//}

	//void OnVolumePlusClicked(object? sender, EventArgs e)
	//{
	//	if (mediaElement.Volume < 1)
	//	{
	//		if (mediaElement.Volume > .9)
	//		{
	//			mediaElement.Volume = 1;

	//			return;
	//		}

	//		mediaElement.Volume += .1;
	//	}
	//}

	//void OnPlayClicked(object? sender, EventArgs e)
	//{
	//	mediaElement.Play();
	//}

	//void OnPauseClicked(object? sender, EventArgs e)
	//{
	//	mediaElement.Pause();
	//}
	void OnPlayClicked(object? sender, EventArgs e)
	{
		if (mediaElement.CurrentState == MediaElementState.Playing)
		{
			mediaElement.Pause();
			playButton.Source = "playicon02.png";
		}
		else
		{
			mediaElement.Play();
			playButton.Source = "pauseicon02.png";
		}
        isControlsVisible = true;
        SetControlsVisibility();
    }

	void OnStopClicked(object? sender, EventArgs e)
	{
		mediaElement.Stop();
	}

	//void OnMuteClicked(object? sender, EventArgs e)
	//{
	//	mediaElement.ShouldMute = !mediaElement.ShouldMute;
	//}

	void OnMuteClicked(object? sender, EventArgs e)
	{
		mediaElement.ShouldMute = !mediaElement.ShouldMute;

		// Update icon image and set volume based on the mute status
		if (mediaElement.ShouldMute)
		{
			muteButton.Source = "muteicon.png";
			volumeSlider.IsEnabled = false;
			mediaElement.Volume = 0;
		}
		else
		{
			muteButton.Source = "volumeicon.png";
			volumeSlider.IsEnabled = true;
			mediaElement.Volume = volumeSlider.Value;
		}
        isControlsVisible = true;
        SetControlsVisibility();
    }

	void BasePage_Unloaded(object? sender, EventArgs e)
	{
		// Stop and cleanup MediaElement when we navigate away
		mediaElement.Handler?.DisconnectHandler();
	}

	void Slider_DragCompleted(object? sender, EventArgs e)
	{
		ArgumentNullException.ThrowIfNull(sender);

		var newValue = ((Slider)sender).Value;
		mediaElement.SeekTo(TimeSpan.FromSeconds(newValue));
		mediaElement.Play();
	}

	void Slider_DragStarted(object sender, EventArgs e)
	{
		mediaElement.Pause();
	}

	void Button_Clicked(object? sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(customSourceEntry.Text))
		{
			DisplayAlert("Error Loading URL Source", "No value was found to load as a media source. " +
				"When you do enter a value, make sure it's a valid URL. No additional validation is done.",
				"OK");

			return;
		}

		mediaElement.Source = MediaSource.FromUri(customSourceEntry.Text);
	}

	//async void ChangeSourceClicked(System.Object sender, System.EventArgs e)
	//{
	//	var result = await DisplayActionSheet("Choose a source", "Cancel", null,
	//		loadOnlineMp4, loadHls, loadLocalResource, resetSource);

	//	switch (result)
	//	{
	//		case loadOnlineMp4:
	//			mediaElement.Source =
	//				MediaSource.FromUri(
	//					"cc");
	//			return;

	//		case loadHls:
	//			mediaElement.Source
	//				= MediaSource.FromUri(
	//					"https://demo.unified-streaming.com/k8s/features/stable/video/tears-of-steel/tears-of-steel.ism/.m3u8");
	//			return;

	//		case resetSource:
	//			mediaElement.Source = null;
	//			return;

	//		case loadLocalResource:
	//			if (DeviceInfo.Platform == DevicePlatform.MacCatalyst
	//				|| DeviceInfo.Platform == DevicePlatform.iOS)
	//			{
	//				mediaElement.Source = MediaSource.FromResource("abc.mp4");
	//			}
	//			else if (DeviceInfo.Platform == DevicePlatform.Android)
	//			{
	//				mediaElement.Source = MediaSource.FromResource("abc.mp4");
	//			}
	//			else if (DeviceInfo.Platform == DevicePlatform.WinUI)
	//			{
	//				mediaElement.Source = MediaSource.FromResource("abc.mp4");
	//			}
	//			return;
	//	}
	//}

	async void ChangeAspectClicked(System.Object sender, System.EventArgs e)
	{
		var resultAspect = await DisplayActionSheet("Choose aspect ratio",
			"Cancel", null, Aspect.AspectFit.ToString(),
			Aspect.AspectFill.ToString(), Aspect.Fill.ToString());

		if (resultAspect.Equals("Cancel"))
		{
			return;
		}

		if (!Enum.TryParse(typeof(Aspect), resultAspect, true, out var aspectEnum)
			|| aspectEnum is null)
		{
			await DisplayAlert("Error", "There was an error determining the selected aspect",
				"OK");

			return;
		}

		mediaElement.Aspect = (Aspect)aspectEnum;
        isControlsVisible = true;
        SetControlsVisibility();
    }
	//private void SetMediaSource(string source)
	//{
	//	switch (source)
	//	{
	//		case loadOnlineMp4:
	//			mediaElement.Source =
	//				MediaSource.FromUri(
	//					"cc");
	//			return;

	//		case loadHls:
	//			mediaElement.Source
	//				= MediaSource.FromUri(
	//					"https://demo.unified-streaming.com/k8s/features/stable/video/tears-of-steel/tears-of-steel.ism/.m3u8");
	//			return;

	//		case resetSource:
	//			mediaElement.Source = null;
	//			return;

	//		case loadLocalResource:
	//			if (DeviceInfo.Platform == DevicePlatform.MacCatalyst
	//				|| DeviceInfo.Platform == DevicePlatform.iOS)
	//			{
	//				mediaElement.Source = MediaSource.FromResource("abc.mp4");
	//			}
	//			else if (DeviceInfo.Platform == DevicePlatform.Android)
	//			{
	//				mediaElement.Source = MediaSource.FromResource("abc.mp4");
	//			}
	//			else if (DeviceInfo.Platform == DevicePlatform.WinUI)
	//			{
	//				mediaElement.Source = MediaSource.FromResource("abc.mp4");
	//			}
	//			return;
	//	}
	//}
	//async void ChangeSourceClicked(object sender, EventArgs e)
	//{
	//	var result = await DisplayActionSheet("Choose a source", "Cancel", null,
	//		loadOnlineMp4, loadHls, loadLocalResource, resetSource);

	//	if (!string.IsNullOrEmpty(result))
	//	{
	//		SetMediaSource(result);
	//	}
	//}


	private void SetMediaSource(MediaSource source)
	{
		mediaElement.Source = source;
	}

	//async void ChangeSourceClicked(object sender, EventArgs e)
	//{
	//	var result = await DisplayActionSheet("Choose a source", "Cancel", null,
	//		loadOnlineMp4, loadHls, loadLocalResource, resetSource);

	//	if (!string.IsNullOrEmpty(result))
	//	{
	//		switch (result)
	//		{
	//			case loadOnlineMp4:
	//				SetMediaSource(MediaSource.FromUri(new SystemUri("cc")));
	//				break;
	//			case loadHls:
	//				SetMediaSource(MediaSource.FromUri(new SystemUri("https://demo.unified-streaming.com/k8s/features/stable/video/tears-of-steel/tears-of-steel.ism/.m3u8")));
	//				break;
	//			case resetSource:
	//				SetMediaSource(null);
	//				break;
	//			case loadLocalResource:
	//				if (DeviceInfo.Platform == DevicePlatform.MacCatalyst
	//					|| DeviceInfo.Platform == DevicePlatform.iOS)
	//				{
	//					SetMediaSource(MediaSource.FromResource("abc.mp4"));
	//				}
	//				else if (DeviceInfo.Platform == DevicePlatform.Android)
	//				{
	//					SetMediaSource(MediaSource.FromResource("abc.mp4"));
	//				}
	//				else if (DeviceInfo.Platform == DevicePlatform.WinUI)
	//				{
	//					SetMediaSource(MediaSource.FromResource("abc.mp4"));
	//				}
	//				break;
	//		}
	//	}
	//}

	private void SetMediaSource(System.Uri videoUri)
	{
		MediaSourceUri = videoUri;
		mediaElement.Source = MediaSource.FromUri(videoUri);

    }
	async void ChangeSourceClicked(object sender, EventArgs e)
	{
		var result = await DisplayActionSheet("Choose a source", "Cancel", null,
			loadOnlineMp4, loadHls, loadLocalResource, resetSource);

		if (!string.IsNullOrEmpty(result))
		{
			switch (result)
			{
				case loadOnlineMp4:
					SetMediaSource(new System.Uri("cc"));
					break;
				case loadHls:
					SetMediaSource(new System.Uri("https://demo.unified-streaming.com/k8s/features/stable/video/tears-of-steel/tears-of-steel.ism/.m3u8"));
					break;
				case resetSource:
					SetMediaSource(null);
					break;
                case loadLocalResource:
					if (DeviceInfo.Platform == DevicePlatform.MacCatalyst
						|| DeviceInfo.Platform == DevicePlatform.iOS)
					{
						SetMediaSource(MediaSource.FromResource("abc.mp4"));
					}
					else if (DeviceInfo.Platform == DevicePlatform.Android)
					{
						SetMediaSource(MediaSource.FromResource("abc.mp4"));
					}
					else if (DeviceInfo.Platform == DevicePlatform.WinUI)
					{
						SetMediaSource(MediaSource.FromResource("abc.mp4"));
					}
					break;
			}
        }
	}

	public System.Uri? MediaSourceUri { get; set; }
	

	private async void btnFullScreen_Clicked(object sender, EventArgs e)
	{
		if (MediaSourceUri != null)
		{
			CurrentVideoState videoState = new CurrentVideoState
			{
				Position = mediaElement.Position,
				VideoUri = MediaSourceUri,
			};
			FullScreenPage page = new FullScreenPage(videoState);

			await MopupService.Instance.PushAsync(page);
		}
        isControlsVisible = true;
        SetControlsVisibility();

    }
    private async void OnFullScreenClosed(object sender, NotifyFullScreenClosed message)
    {
        if (message.Value)
        {
            // Reset the mediaElement position with the one received from FullScreenPage
            mediaElement.SeekTo(message.Position);
            mediaElement.Play();
        }
    }

    private bool isControlsVisible = false;

    private void SetControlsVisibility()
    {
        playButton.IsVisible = isControlsVisible;
        // Set visibility for other controls (e.g., btnFullScreen) as needed
        btnFullScreen.IsVisible = isControlsVisible;
		muteButton.IsVisible = isControlsVisible;
        volumeSlider.IsVisible = isControlsVisible;
		changeAspect.IsVisible = isControlsVisible;
    }

    private async void OnScreenTapped(object sender, EventArgs e)
    {
        isControlsVisible = true;
        SetControlsVisibility();
        await Task.Delay(5000); // Hide controls after 5 seconds
        isControlsVisible = false;
        SetControlsVisibility();
    }





}