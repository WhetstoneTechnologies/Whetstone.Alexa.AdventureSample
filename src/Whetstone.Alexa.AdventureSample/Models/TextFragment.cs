// Copyright (c) 2018 Whetstone Technologies. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
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
