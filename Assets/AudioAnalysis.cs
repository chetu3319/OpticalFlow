using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioAnalysis : MonoBehaviour
{
    /*Frequency bands for
     * Sub Bass: 20 to 60 Hz
     * Bass 60: to 250 Hz
     * Low-Mids: 250 to 500 Hz
     * Mids: 500 to 2kHz
     * Upper Mids: 2 to 4kHz
     * Presence 4kHz to 6kHz
     * Brilliance 6kHz to 20kHz
     */

    /*
        *** The Frequency Bands ***
     
     * [0]Sub Bass:       0   -   86Hz
     * [1]Bass:          87   -   258Hz
     * [2]Low-Mids:     259   -   602Hz
     * [3]Mids:         603   -  1290Hz
     * [4]Upper-Mids:   1291  -  2666Hz
     * [5]Presence:     2667  -  5418Hz
     * [6]Brilliance:   6419  -  10922Hz
     * [7]Dog Whistle: 10923  -  21930Hz
     
       This comes from Peer Play on YouTube @
       "Audio Visualization - Unity/C# Tutorial"
     */
    #region public variables
    public enum Channel { Stereo, Left, Right };
    public Channel channel = new Channel();
    public float[] audioBand = new float[8];
    public float[] audioBandBuffer = new float[8];
    public float[] freqBand = new float[8];

    // Property binders
    [SerializeReference] PropertyBinder[] _propertyBinders = null;
    public PropertyBinder[] propertyBinders
    {
        get => (PropertyBinder[])_propertyBinders.Clone();
        set => _propertyBinders = value;
    }


    public float amplitude = 0, amplitudeBuffer = 0;


    #endregion

    #region private variables

    private AudioSource audioSource;


    float[] samplesLeft = new float[512];
    float[] samplesRight = new float[512];
    float[] bandBuffer = new float[8];
    float[] bufferDecrease = new float[8];
    float[] freqBandHighest = new float[8];
    float AmplitudeHighest;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        //_audioSource = GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {
        if (audioSource == null)
            return;
        GetSpectrumAudioSource();
        MakeFrequencyBands();
        BandBuffer();
        CreateAudioBands();
        GetAmplitude();
        // Output
        if (_propertyBinders != null)
            foreach (var b in _propertyBinders) b.Level = amplitude;
    }


    void GetSpectrumAudioSource()
    {
        audioSource.GetSpectrumData(samplesLeft, 0, FFTWindow.Blackman);
        audioSource.GetSpectrumData(samplesRight, 0, FFTWindow.Blackman);
    }

    void MakeFrequencyBands()
    {
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            float average = 0;
            int sampleCount = (int)Mathf.Pow(2, i) * 2;
            if (i == 7)
            {
                sampleCount += 2;
            }
            for (int j = 0; j < sampleCount; j++)
            {
                if (channel == Channel.Stereo)
                {
                    average += samplesLeft[count] + samplesRight[count] * (count + 1);
                }
                if (channel == Channel.Left)
                {
                    average += samplesLeft[count] * (count + 1);
                }
                if (channel == Channel.Right)
                {
                    average += samplesLeft[count] * (count + 1);
                }
                count++;
            }

            average /= count;
            freqBand[i] = average * 10;
        }
    }

    //This creates a smooth downfall when the amplitude is lower than the previous value, this is the impression that
    //the audio signal is pushing up the blocks and there's almost like an air cushion inside of them as they ease down
    void BandBuffer()
    {
        for (int g = 0; g < 8; ++g)
        {
            if (freqBand[g] > bandBuffer[g])
            {
                bandBuffer[g] = freqBand[g];
                bufferDecrease[g] = 0.005f;
            }
            if (freqBand[g] < bandBuffer[g])
            {
                bandBuffer[g] -= bufferDecrease[g];
                bufferDecrease[g] *= 1.2f;
            }
        }
    }

    void CreateAudioBands()
    {
        for (int i = 0; i < 8; i++)
        {
            if (freqBand[i] > freqBandHighest[i])
            {
                freqBandHighest[i] = freqBand[i];
            }
            audioBand[i] = (freqBand[i] / freqBandHighest[i]);
            audioBandBuffer[i] = (bandBuffer[i] / freqBandHighest[i]);
        }
    }


    void GetAmplitude()
    {
        float _CurrentAmplitude = 0;
        float _CurrentAmplitudeBuffer = 0;
        for (int i = 0; i < 8; i++)
        {
            _CurrentAmplitude += audioBand[i];
            _CurrentAmplitudeBuffer += audioBandBuffer[i];
        }
        if (_CurrentAmplitude > AmplitudeHighest)
        {
            AmplitudeHighest = _CurrentAmplitude;
        }
        amplitude = _CurrentAmplitude / AmplitudeHighest;
        amplitudeBuffer = _CurrentAmplitudeBuffer / AmplitudeHighest;
    }
}
