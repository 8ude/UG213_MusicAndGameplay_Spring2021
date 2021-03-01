using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// RelativeValues of Beats
/// </summary>
public enum TickValue
{
    ThirtySecond = 12,
    SixteenthTriplet = 16,
    Sixteenth = 24,
    EighthTriplet = 32,
    Eighth = 48,
    QuarterTriplet = 64,
    Quarter = 96,
    Half = 192,
    Measure = 384,
    Max = 385
};

/// <summary>
/// RelativeValues of Beats
/// </summary>
public enum BeatValue
{
    ThirtySecond,
    Sixteenth = 2,
    Eighth = 4,
    Quarter = 8,
    Half = 16,
    Measure = 32,
    Max = 33
};


/// <summary>
/// Arguments for Beat Events
/// </summary>
public class BeatArgs
{
    public TickValue BeatVal;
    public double BeatTime;
    public double NextBeatTime;
    public TickMask<bool> TickMask;

    public BeatArgs(TickValue beatVal, double beatTime, double nextBeatTime, TickMask<bool> beatTickMask)
    {
        BeatVal = beatVal;
        BeatTime = beatTime;
        NextBeatTime = nextBeatTime;
        TickMask = beatTickMask;
    }
}

/// <summary>
/// Collections with beat value indexers
/// </summary>
/// <typeparam name="T">Type of beat mask, herein bool</typeparam>
public class TickMask<T>
{
    private readonly T[] _beatMaskArr = new T[(int)TickValue.Max];

    public void Clear()
    {
        Array.Clear(_beatMaskArr, 0, (int)TickValue.Max);
    }

    public T this[TickValue i]
    {
        get
        {
            return _beatMaskArr[(int)i];
        }
        set
        {
            _beatMaskArr[(int)i] = value;
        }
    }
}

/// <summary>
/// Collections with beat value indexers
/// </summary>
/// <typeparam name="T">Type of beat mask, herein bool</typeparam>
public class BeatMask<T>
{
    private readonly T[] _beatMaskArr = new T[(int)BeatValue.Max];

    public void Clear()
    {
        Array.Clear(_beatMaskArr, 0, (int)BeatValue.Max);
    }

    public T this[BeatValue i]
    {
        get
        {
            return _beatMaskArr[(int)i];
        }
        set
        {
            _beatMaskArr[(int)i] = value;
        }
    }
}


[RequireComponent(typeof(AudioSource))]
public class SimpleClock : MonoBehaviour
{
    private static SimpleClock _instance;
    public static SimpleClock Instance { get { return _instance; } }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        if (BPM.Equals(0.0)) Debug.LogWarning("BPM not set! Please set the BPM in the Beat Clock.");
    }

    public double StartDelay;

    [Header("M:B:T")] [SerializeField] private int Measures; //for display purposes in unity editor
    [SerializeField] private int Beats;
    [SerializeField] private int Ticks;

    [Header("Tempo")] public double BPM;

    [Header("Time Signature")] public int beatsPerMeasure;

    private int beatUnit = 4;
    //Not ready for primetime yet, need to massage distinctions between ticks & 32nd notes for it to work reliably
    //American Music notation is terrible at making this clear, and whole note, etc. becomes ambiguous

    [Header("Messages")]
    public bool ThirtySecondMessage;
    public bool SixteenthMessage;
    public bool EighthMessage;
    public bool BeatMessage;
    public bool HalfMessage;
    public bool MeasureMessage;

    private double _sampleRate = 0.0;



    public BeatMask<bool> ThisBeatMask = new BeatMask<bool>();
    public TickMask<bool> TickMask = new TickMask<bool>();
    private TickMask<bool>[] _tickMaskArray = new TickMask<bool>[384];

    private const int TicksPerBeat = 96;
    private double SamplesPerBeat;
    private int _tickCounter;
    private int _ticksPerMeasure;
    private int _thirtySecondCount;
    private int _lastThirtySecondCount;
    private  double SamplesPerTick;
    private int _measureCount;

    private static double TickLength;
    private static double _thirtySecondLength;
    private static double _sixteenthLength;
    private static double _sixteenthTripletLength;
    private static double _eighthLength;
    private static double _eighthTripletLength;
    private static double _quarterLength;
    private static double _quarterTripletLength;
    private static double _halfLength;
    private static double _measureLength;
    private static double _secondsPerMeasure;

    private static double _nextTick = System.Double.MaxValue;
    private static double _nextThirtySecond = System.Double.MaxValue;
    private static double _nextSixteenth;
    private static double _nextSixteenthTriplet;
    private static double _nextEighth;
    private static double _nextEighthTriplet;
    private static double _nextQuarter;
    private static double _nextQuarterTriplet;
    private static double _nextHalf;
    private static double _nextMeasure;
    private bool _initialized;

    private double _dspTimeAtTick;

    public delegate void BeatEvent(BeatArgs args);

    /// <summary>
    /// Event sent every 32nd note
    /// </summary>
    public static event BeatEvent ThirtySecond;

    /// <summary>
    /// Event sent every 16th note
    /// </summary>
    public static event BeatEvent Sixteenth;

    /// <summary>
    /// Event sent every 8th note
    /// </summary>
    public static event BeatEvent Eighth;

    /// <summary>
    /// Event sent every beat (1/4 note)
    /// </summary>
    public static event BeatEvent Beat;

    /// <summary>
    /// Event sent every 32nd note
    /// </summary>
    public static event BeatEvent Measure;

    /// <summary>
    /// Creates a tick mask array based on the number of ticks per measure
    /// </summary>
    void BuildTickMaskArray()
    {
        _tickMaskArray = new TickMask<bool>[_ticksPerMeasure + 1];
        for (int i = 1; i <= _ticksPerMeasure; i++)
        {
            _tickMaskArray[i] = new TickMask<bool>();
            if (i % (int)TickValue.ThirtySecond == 0)
                _tickMaskArray[i][TickValue.ThirtySecond] = true;
            if (i % (int)TickValue.SixteenthTriplet == 0)
                _tickMaskArray[i][TickValue.SixteenthTriplet] = true;
            if (i % (int)TickValue.Sixteenth == 0)
                _tickMaskArray[i][TickValue.Sixteenth] = true;
            if (i % (int)TickValue.EighthTriplet == 0)
                _tickMaskArray[i][TickValue.EighthTriplet] = true;
            if (i % (int)TickValue.Eighth == 0)
                _tickMaskArray[i][TickValue.Eighth] = true;
            if (i % (int)TickValue.QuarterTriplet == 0)
                _tickMaskArray[i][TickValue.QuarterTriplet] = true;
            if (i % (int)TickValue.Quarter == 0)
                _tickMaskArray[i][TickValue.Quarter] = true;
            if (i % (int)TickValue.Half == 0)
                _tickMaskArray[i][TickValue.Half] = true;
            if (i % (int)TickValue.Measure == 0)
                _tickMaskArray[i][TickValue.Measure] = true;
        }
    }


    /// <summary>
    /// Creates a beat mask array based on the number of ticks per measure
    /// </summary>
    void UpdateBeats()
    {
        ThisBeatMask.Clear();
        if (ThirtySecondMessage) BroadcastMessage("ThirtySecond");
        ThisBeatMask[BeatValue.ThirtySecond] = true;
        if (_thirtySecondCount % 2 != 0) return;
        if (SixteenthMessage) BroadcastMessage("Sixteenth");
        ThisBeatMask[BeatValue.Sixteenth] = true;
        if (_thirtySecondCount % 4 != 0) return;
        if (EighthMessage) BroadcastMessage("Eighth");
        ThisBeatMask[BeatValue.Eighth] = true;
        if (_thirtySecondCount % 8 != 0) return;
        if (BeatMessage) BroadcastMessage("Beat");
        ThisBeatMask[BeatValue.Quarter] = true;
        if (_thirtySecondCount % 16 != 0) return;
        if (HalfMessage) BroadcastMessage("Half");
        ThisBeatMask[BeatValue.Half] = true;
        if (_thirtySecondCount % 32 != 0) return;
        if (MeasureMessage) BroadcastMessage("Measure");
        ThisBeatMask[BeatValue.Measure] = true;
    }

    /// <summary>
    /// Set a new BPM.
    /// </summary>
    /// <param name="newBPM">New BPM, can be int, float, or double</param>
    public void SetBPM(int newBPM)
    {
        double BPMdbl = (double)newBPM;
        InitializeBPM(BPMdbl);
    }

    /// <summary>
    /// Set a new BPM.
    /// </summary>
    /// <param name="newBPM">New BPM, can be int, float, or double</param>
    public void SetBPM(float newBPM)
    {
        double BPMdbl = (double)newBPM;
        InitializeBPM(BPMdbl);
    }

    /// <summary>
    /// Set a new BPM.
    /// </summary>
    /// <param name="newBPM">New BPM, can be int, float, or double</param>
    public void SetBPM(double NewBPM)
    {
        InitializeBPM((NewBPM));
    }

    /// <summary>
    /// Internal function to intitialize a new BPM.
    /// </summary>
    /// <param name="newBPM">BPM to be initialzed</param>
    void InitializeBPM(double NewBPM)
    {
        _initialized = false;
        ResetBeatCounts();
        //if (NewBPM > 210)
        //{
        //    NewBPM = NewBPM / 2;
        //    Debug.LogWarning("New BPM set > 210. Clock loses stability above ~210. Dividing BPM in half, using " + NewBPM + " instead.");
        //}
        BPM = NewBPM;

        //running = true;
        _secondsPerMeasure = 60 / BPM * beatsPerMeasure;
        _ticksPerMeasure = TicksPerBeat * beatsPerMeasure;
        _tickCounter = _ticksPerMeasure;
        SetLengths();
        BuildTickMaskArray();
        FirstBeat();
    }

    /// <summary>
    /// Internal function to set lengths of each beat value
    /// </summary>
    void SetLengths()
    {
        _measureLength = _secondsPerMeasure;
        _sampleRate = AudioSettings.outputSampleRate;
        SamplesPerTick = _sampleRate * (_secondsPerMeasure / _ticksPerMeasure);
        SamplesPerBeat = SamplesPerTick * (_ticksPerMeasure / beatsPerMeasure);
        TickLength = _measureLength / _ticksPerMeasure;
        _thirtySecondLength = TickLength * (int)TickValue.ThirtySecond;
        _sixteenthTripletLength = TickLength * (int)TickValue.SixteenthTriplet;
        _sixteenthLength = TickLength * (int)TickValue.Sixteenth;
        _eighthTripletLength = TickLength * (int)TickValue.EighthTriplet;
        _eighthLength = TickLength * (int)TickValue.Eighth;
        _quarterTripletLength = TickLength * (int)TickValue.QuarterTriplet;
        _quarterLength = TickLength * (int)TickValue.Quarter;
        _halfLength = TickLength * (int)TickValue.Half;
    }

    /// <summary>
    /// Internal function, sets timings for next beats
    /// </summary>
    void FirstBeat()
    {
        double startTick = AudioSettings.dspTime + StartDelay;
        _nextTick = startTick * _sampleRate + SamplesPerTick; //_tickLength;
        _nextThirtySecond = startTick + _thirtySecondLength;
        _nextSixteenthTriplet = startTick + _sixteenthTripletLength;
        _nextSixteenth = startTick + _sixteenthLength;
        _nextEighthTriplet = startTick + _eighthTripletLength;
        _nextEighth = startTick + _eighthLength;
        _nextQuarterTriplet = startTick + _eighthTripletLength;
        _nextQuarter = startTick + _quarterLength;
        _nextHalf = startTick + _halfLength;
        _nextMeasure = startTick + _measureLength;
        _initialized = true;
    }

    /// <summary>
    /// Resets counts for all beats
    /// </summary>
    void ResetBeatCounts()
    {
        _tickCounter = 1;
        _measureCount = 1;
    }

    /// <summary>
    /// Initilizes & starts Clock on Start()
    /// </summary>
    void Start()
    {
        Application.runInBackground = true;
        if (!BPM.Equals(0.0))
        {
            InitializeBPM(BPM);
        }
        else
        {
            Debug.LogWarning("BPM not set or set to 0, Clock not initialized on load.");
        }
    }

    void Update()
    {
        if (_thirtySecondCount != _lastThirtySecondCount)
        {
            UpdateBeats();
            _lastThirtySecondCount = _thirtySecondCount;
        }
    }

    /// <summary>
    /// Using OnAudioFilter Read to updates beats & dispatches events accordingly.
    /// </summary>
    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!_initialized) return;
        double sample = AudioSettings.dspTime * _sampleRate;
        int dataLen = data.Length / channels;
        int n = 0;
        while (n < dataLen)
        {
            if (sample + n >= _nextTick)
            {
                _nextTick += SamplesPerTick;
                if (++_tickCounter > _ticksPerMeasure)
                {
                    _tickCounter = 1;
                    _measureCount++;
                }
                UpdateTicks();
                Measures = _measureCount;
                Beats = 1 + _tickCounter / (TicksPerBeat);
                Ticks = 1 + _tickCounter % (TicksPerBeat);
            }
            n++;
        }
    }

    /// <summary>
    /// Function to update beat counts, called after every new tick
    /// </summary>
    void UpdateTicks()
    {
        TickMask = _tickMaskArray[_tickCounter];
        if (TickMask[TickValue.ThirtySecond])
        {
            if (ThirtySecond != null)
                ThirtySecond(new BeatArgs(TickValue.ThirtySecond, _nextThirtySecond,
                    _nextThirtySecond + _thirtySecondLength, TickMask));
            _nextThirtySecond += _thirtySecondLength;
            _thirtySecondCount++;
        }
        if (TickMask[TickValue.SixteenthTriplet])
        {
            _nextSixteenthTriplet += _sixteenthTripletLength;
        }
        if (TickMask[TickValue.Sixteenth])
        {
            if (Sixteenth != null)
                Sixteenth(new BeatArgs(TickValue.Sixteenth, _nextSixteenth,
                    _nextSixteenth + _sixteenthLength, TickMask));
            _nextSixteenth += _sixteenthLength;
        }
        if (TickMask[TickValue.EighthTriplet])
        {
            _nextEighthTriplet += _eighthTripletLength;
        }
        if (TickMask[TickValue.Eighth])
        {
            if (Eighth != null)
                Eighth(new BeatArgs(TickValue.Eighth, _nextEighth,
                    _nextEighth + _eighthLength, TickMask));
            _nextEighth += _eighthLength;
        }
        if (TickMask[TickValue.QuarterTriplet])
        {
            _nextQuarterTriplet += _quarterTripletLength;
        }
        if (TickMask[TickValue.Quarter])
        {
            if (Beat != null)
                Beat(new BeatArgs(TickValue.Quarter, _nextQuarter,
                   _nextQuarter + _quarterLength, TickMask));
            _nextQuarter += _quarterLength;
        }
        if (TickMask[TickValue.Half])
        {
            _nextHalf += _halfLength;
        }
        if (TickMask[TickValue.Measure])
        {
            if (Measure != null)
                Measure(new BeatArgs(TickValue.Measure, _nextMeasure,
                    _nextMeasure + _measureLength, TickMask));
            _nextMeasure += _measureLength;
            //_thirtySecondCount = 0;
        }
    }

    /// <summary>
    /// Returns time of next specified value Usage: AudioSource.PlayScheduled(AtNext(Beat.TickValue.Quarter))
    /// </summary>
    /// <param name="beatTickValue">TickValue for which to get time of next</param>
    /// <returns>Time of next specified value</returns>
    public static double AtNext(TickValue beatTickValue)
    {
        switch (beatTickValue)
        {
            case TickValue.ThirtySecond:
                return AtNextThirtySecond();
            case TickValue.SixteenthTriplet:
                return AtNextSixteenthTriplet();
            case TickValue.Sixteenth:
                return AtNextSixteenth();
            case TickValue.EighthTriplet:
                return AtNextEighthTriplet();
            case TickValue.Eighth:
                return AtNextEighth();
            case TickValue.QuarterTriplet:
                return AtNextQuarterTriplet();
            case TickValue.Quarter:
                return AtNextQuarter();
            case TickValue.Half:
                return AtNextHalf();
            case TickValue.Measure:
                return AtNextMeasure();
            default:
                return AtNextBeat();
        }
    }

    /// <summary>
    /// Time of next 32nd note. Usage: AudioSource.PlayScheduled(AtNextThirtySecond())
    /// </summary>
    /// <returns>Time of next 32nd note as double</returns>
    public static double AtNextThirtySecond()
    {
        return _nextThirtySecond;
    }
    /// <summary>
    /// Time of next 16th note triplet. Usage: AudioSource.PlayScheduled(AtNextSixteenth())
    /// </summary>
    /// <returns>Time of next 16th note triplet as double</returns>
    public static double AtNextSixteenthTriplet()
    {
        return _nextSixteenth;
    }
    /// <summary>
    /// Time of next 16th note. Usage: AudioSource.PlayScheduled(AtNextSixteenth())
    /// </summary>
    /// <returns>Time of next 16th note as double</returns>
    public static double AtNextSixteenth()
    {
        return _nextSixteenth;
    }
    /// <summary>
    /// Time of next 8th note triplet. Usage: AudioSource.PlayScheduled(AtNextEighth())
    /// </summary>
    /// <returns>Time of next 8th note as double</returns>
    public static double AtNextEighthTriplet()
    {
        return _nextEighthTriplet;
    }
    /// <summary>
    /// Time of next 8th note. Usage: AudioSource.PlayScheduled(AtNextEighth())
    /// </summary>
    /// <returns>Time of next 8th note as double</returns>
    public static double AtNextEighth()
    {
        return _nextEighth;
    }
    /// <summary>
    /// Time of next 1/4 note triplet. Usage: AudioSource.PlayScheduled(AtNextQuarter())
    /// </summary>
    /// <returns>Time of next 1/4 note triplet as double</returns>
    public static double AtNextQuarterTriplet()
    {
        return _nextQuarterTriplet;
    }
    /// <summary>
    /// Time of next 1/4 note. Usage: AudioSource.PlayScheduled(AtNextQuarter())
    /// </summary>
    /// <returns>Time of next 1/4 note as double</returns>
    public static double AtNextQuarter()
    {
        return _nextQuarter;
    }
    /// <summary>
    /// Time of next beat (1/4 note). Usage: AudioSource.PlayScheduled(AtNextBeat())
    /// </summary>
    /// <returns>Time of next beat (1/4 note) as double</returns>
    public static double AtNextBeat()
    {
        return _nextQuarter;
    }
    /// <summary>
    /// Time of next 1/2 note. Usage: AudioSource.PlayScheduled(AtNextHalf())
    /// </summary>
    /// <returns>Time of next 1/2 note as double</returns>
    public static double AtNextHalf()
    {
        return _nextHalf;
    }
    /// <summary>
    /// Time of next measure. Usage: AudioSource.PlayScheduled(AtNextMeasure())
    /// </summary>
    /// <returns>Time of next measure as double</returns>
    public static double AtNextMeasure()
    {
        return _nextMeasure;
    }

    /// <summary>
    /// Helper fucntions for timing things like tweens & animations to the beat clock
    /// </summary>
    /// <param name="beatTickValue"></param>
    /// <returns></returns>
    public static float LengthOf(TickValue beatTickValue)
    {
        switch (beatTickValue)
        {
            case TickValue.ThirtySecond:
                return ThirtySecondLength();
            case TickValue.SixteenthTriplet:
                return SixteenthTripletLength();
            case TickValue.Sixteenth:
                return SixteenthLength();
            case TickValue.EighthTriplet:
                return EighthTripletLength();
            case TickValue.Eighth:
                return EighthLength();
            case TickValue.QuarterTriplet:
                return QuarterTripletLength();
            case TickValue.Quarter:
                return QuarterLength();
            case TickValue.Half:
                return HalfLength();
            case TickValue.Measure:
                return MeasureLength();
            default:
                return BeatLength();
        }
    }

    public static float ThirtySecondLength()
    {
        return (float)_thirtySecondLength;
    }

    public static float SixteenthTripletLength()
    {
        return (float)_sixteenthTripletLength;
    }

    public static float SixteenthLength()
    {
        return (float)_sixteenthLength;
    }

    public static float EighthTripletLength()
    {
        return (float)_eighthTripletLength;
    }

    public static float EighthLength()
    {
        return (float)_eighthLength;
    }

    public static float QuarterTripletLength()
    {
        return (float)_quarterTripletLength;
    }

    public static float QuarterLength()
    {
        return (float)_quarterLength;
    }

    public static float BeatLength()
    {
        return (float)_quarterLength;
    }

    public static float HalfLength()
    {
        return (float)_halfLength;
    }

    public static float MeasureLength()
    {
        return (float)_measureLength;
    }

    /// <summary>
    /// Helper functions for timing precise audio playback to the beatclock
    /// </summary>
    /// <param name="beatTickValue"></param>
    /// <returns>Returns length of the beat value</returns>
    public static double LengthOfD(TickValue beatTickValue)
    {
        switch (beatTickValue)
        {
            case TickValue.ThirtySecond:
                return ThirtySecondLengthD();
            case TickValue.SixteenthTriplet:
                return SixteenthTripletLengthD();
            case TickValue.Sixteenth:
                return SixteenthLengthD();
            case TickValue.EighthTriplet:
                return EighthTripletLengthD();
            case TickValue.Eighth:
                return EighthLengthD();
            case TickValue.QuarterTriplet:
                return QuarterTripletLengthD();
            case TickValue.Quarter:
                return QuarterLengthD();
            case TickValue.Half:
                return HalfLengthD();
            case TickValue.Measure:
                return MeasureLengthD();
            default:
                return BeatLengthD();
        }
    }

    public static double ThirtySecondLengthD()
    {
        return _thirtySecondLength;
    }

    public static double SixteenthTripletLengthD()
    {
        return _sixteenthTripletLength;
    }

    public static double SixteenthLengthD()
    {
        return _sixteenthLength;
    }

    public static double EighthTripletLengthD()
    {
        return _eighthTripletLength;
    }

    public static double EighthLengthD()
    {
        return _eighthLength;
    }

    public static double QuarterTripletLengthD()
    {
        return _quarterTripletLength;
    }

    public static double QuarterLengthD()
    {
        return _quarterLength;
    }

    public static double BeatLengthD()
    {
        return _quarterLength;
    }

    public static double HalfLengthD()
    {
        return _halfLength;
    }

    public static double MeasureLengthD()
    {
        return _measureLength;
    }
}
