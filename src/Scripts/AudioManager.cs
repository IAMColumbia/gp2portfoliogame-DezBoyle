using Godot;
using System;

public partial class AudioManager : Node
{
	public float MasterVolume
	{
		get { return Mathf.DbToLinear(AudioServer.GetBusVolumeDb(1)); }
		set { AudioServer.SetBusVolumeDb(0, Mathf.LinearToDb(Mathf.Clamp(value, 0f, 1f))); }
	}

	[Export] private bool disableMusic = false;
	[Export] private AudioStream music;

	private AudioStreamPlayer musicPlayer;

    public override void _Ready()
    {
		if(disableMusic)
        { return; }
        musicPlayer = new AudioStreamPlayer();
		MasterVolume = 0.5f;
		AddChild(musicPlayer);
		musicPlayer.Stream = music;
		musicPlayer.VolumeDb = Mathf.LinearToDb(0.5f);
		musicPlayer.Play();
    }
}
