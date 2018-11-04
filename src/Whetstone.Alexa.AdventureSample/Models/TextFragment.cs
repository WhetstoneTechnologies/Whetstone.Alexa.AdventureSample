using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.Alexa.AdventureSample.Models
{
    public enum SpeechFramgementType
    {
        Text = 1,
        AudioFile = 2,
        AudioLibrary =3

    }


    public class SpeechFragement
    {

        public SpeechFramgementType FragmentType { get; set; }

        public string AudioFileName { get; set; }


        // Uses a file from the Amazon audio library.
        public string AudioLibraryTag { get; set; }


        public string Text { get; set; }


        public string VoiceId { get; set; }

        public static SpeechFragement CreateTextFragment(string speechText)
        {
            SpeechFragement frag = new SpeechFragement();

            frag.Text = speechText;
            frag.FragmentType = SpeechFramgementType.Text;

            return frag;
        }

        public static SpeechFragement CreateTextFragment(string speechText, string voiceId)
        {
            SpeechFragement frag = new SpeechFragement();

            frag.Text = speechText;
            frag.FragmentType = SpeechFramgementType.Text;
            frag.VoiceId = voiceId;
            return frag;
        }

        public static SpeechFragement CreateAudioFragment(string audioFile)
        {
            SpeechFragement frag = new SpeechFragement();

            frag.AudioFileName = audioFile;
            frag.FragmentType = SpeechFramgementType.AudioFile;

            return frag;
        }


        public static SpeechFragement CreateAudioLibraryFragment(string audioLibTag)
        {
            SpeechFragement frag = new SpeechFragement();

            frag.AudioLibraryTag = audioLibTag;
            frag.FragmentType = SpeechFramgementType.AudioLibrary;

            return frag;
        }

    }
}
