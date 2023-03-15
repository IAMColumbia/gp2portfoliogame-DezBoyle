using Godot;
using System;
using System.Collections.Generic;

public partial class AudioPlayer : Node3D
{
	private AudioStreamPlayer3D currentAudio;

	public override void _EnterTree()
	{
		NewAudio();
	}

	public void PlayRandom(Godot.Collections.Array<AudioStream> streams, float volumeDb)
	{
		PlayOneShot(streams[Utility.RandomRange(0, streams.Count)], volumeDb);
	}

    public void PlayOneShot(AudioStream stream, float volumeDb) { PlayOneShot(stream, volumeDb, 1f); }
	public void PlayOneShot(AudioStream stream, float volumeDb, float pitch)
	{
		currentAudio.Stream = stream;
		currentAudio.Play();
		currentAudio.VolumeDb = volumeDb - 100; //bug in godot.  100 = 0Db apparently
		currentAudio.PitchScale = pitch;
		currentAudio.Finished += currentAudio.QueueFree;
		NewAudio();
	}

	private void NewAudio()
	{
		currentAudio = new AudioStreamPlayer3D();
		AddChild(currentAudio);
	}
}
