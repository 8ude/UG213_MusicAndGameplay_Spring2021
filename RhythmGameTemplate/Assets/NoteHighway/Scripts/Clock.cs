using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

using System;

namespace Beat
{

    [Serializable]
    //MBT stands for "Measure - Beat - Tick"  A tick here is 1/96 of a beat.
    public class MBT : IComparable
    {
        public int Measure;
        public int Beat;
        public int Tick;

        public MBT(int measure, int beat, int tick)
        {
            Measure = measure;
            Beat = beat;
            Tick = tick;
        }

        public int GetTickValue()
        {
            //Measures, Beats and Ticks all start at 1, so we aren't counting them
            return ((Measure-1) * (Clock.Instance.beatsPerMeasure * 96)) + ((Beat-1) * 96) + (Tick);
        }

        //Use this after start is called to get the time in milliseconds of this MBT
        public double GetMilliseconds()
        {

            double millis = 1000 * (GetTickValue()*(Clock.Instance.SamplesPerTick/AudioSettings.outputSampleRate));

            return millis;
        }

        public override string ToString()
        {
            string value = "M:" + Measure + " B: " + Beat + " T: " + Tick;
            return value;
        }

        public int CompareTo(object obj)
        {
            MBT objMBT = (MBT)obj;
            int objTicks = objMBT.Measure * (4 * 96) + (objMBT.Beat * 96) + objMBT.Tick;
            int thisTicks = this.Measure * (4 * 96) + (this.Beat * 96) + this.Tick;
            if (thisTicks == objTicks)
            {
                return 0;
            }
            else if (thisTicks > objTicks)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
    }


    //Possible Improvement - variable tick length 
    //(standard for computer MIDI devices is 960 PPQN, but 96 equates to 1/384 note, which is fine for the purposes of rhythm games)

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
    public class Args
    {
        public TickValue BeatVal;
        public double BeatTime;
        public double NextBeatTime;
        public TickMask<bool> TickMask;

        public Args(TickValue beatVal, double beatTime, double nextBeatTime, TickMask<bool> beatTickMask)
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
    public class Clock : Singleton<Clock>
    {

        public class WaitForBeatSync : CustomYieldInstruction
        {
            private BeatValue _waitBeatValue;
            private int currentThirtySecond;

            public override bool keepWaiting
            {
                get { return (!Clock.Instance.ThisBeatMask[_waitBeatValue] || currentThirtySecond == Clock.Instance._thirtySecondCount); }
            }

            public WaitForBeatSync(BeatValue wtBeatValue)
            {
                this._waitBeatValue = wtBeatValue;
                this.currentThirtySecond = Clock.Instance._thirtySecondCount;
                //print("Starting Tick: " + currentTick);
            }
        }

        //not sure why this is here...
        protected Clock()
        {
        }

        void Awake()
        {
            if (BPM.Equals(0.0)) Debug.LogWarning("_bpm not set! Please set the _bpm in the Beat Clock.");
            //DontDestroyOnLoad(this);
            Application.runInBackground = true;

        }

        public double StartDelay;

        [Header("M:B:T")] [SerializeField] private int Measures; //for display purposes in unity editor
        [SerializeField] private int Beats;
        [SerializeField] private int Ticks;

        [Header("Tempo")] public double BPM;

        [Header("Time Signature")] public int beatsPerMeasure;

        public int beatUnit = 4;
        //Not ready for primetime yet, need to massage distinctions between ticks & 32nd notes for it to work reliably
        //Honestly, American Music notation is terrible at making this clear, and whole note, etc. becomes ambiguous

        [Header("Accuracy Benchmarks (in ms)")] [SerializeField] private int AverageLatency;
        [SerializeField] private int AverageJitter;

        private double _sampleRate = 0.0;

        public BeatMask<bool> ThisBeatMask = new BeatMask<bool>();
        public TickMask<bool> TickMask = new TickMask<bool>();
        private TickMask<bool>[] _tickMaskArray = new TickMask<bool>[384];

        public double Time;
        public double TimeMS;

        public double TimeMSAdjusted;

        public bool Running = false;
        private double _startTime;
        public double startTime
        {
            get { return _startTime; }
        }

        public const int TicksPerBeat = 96;
        public double SamplesPerBeat;
        public int _tickCounter;
        private int _ticksPerMeasure;
        private int _thirtySecondCount;
        private int _lastThirtySecondCount;
        public double SamplesPerTick;
        private int _measureCount;

        public double TickLength;
        private double _thirtySecondLength;
        private double _sixteenthLength;
        private double _sixteenthTripletLength;
        private double _eighthLength;
        private double _eighthTripletLength;
        private double _quarterLength;
        private double _quarterTripletLength;
        private double _halfLength;
        private double _measureLength;
        private double _secondsPerMeasure;

        private double _nextTick = System.Double.MaxValue;
        private double _nextThirtySecond = System.Double.MaxValue;
        private double _nextSixteenth;
        private double _nextSixteenthTriplet;
        private double _nextEighth;
        private double _nextEighthTriplet;
        private double _nextQuarter;
        private double _nextQuarterTriplet;
        private double _nextHalf;
        private double _nextMeasure;
        private bool _initialized;

        private bool TestBool;


        private double _dspTimeAtTick;
        private int _latencyCounter;
        private double _latencyCompensation = 0;
        private double _rollingAverageOfLatency;
        private double _standardDeviationOfLatency;

        private double[] latency = new double[25];

        //adjustment for de-sync
        private double[] timeMSBuffer = new double[10];
        private int _msBufferIndex = 0;

        public int latencyCalculationInterval;
        public AudioSource Source;

        public delegate void CustomUpdate();

        public event CustomUpdate AudioUpdate;

        public delegate void BeatEvent(Args args);

        /// <summary>
        /// Event sent every 32nd note
        /// </summary>
        public event BeatEvent ThirtySecond;

        /// <summary>
        /// Event sent every 16th note
        /// </summary>
        public event BeatEvent Sixteenth;

        /// <summary>
        /// Event sent every 8th note
        /// </summary>
        public event BeatEvent Eighth;

        /// <summary>
        /// Event sent every beat (1/4 note)
        /// </summary>
        public event BeatEvent Beat;

        /// <summary>
        /// Event sent every measure
        /// </summary>
        public event BeatEvent Measure;

        /// <summary>
        /// Creates a beat mask array based on the number of ticks per measure
        /// </summary>
        void BuildBeatMaskArray()
        {
            _tickMaskArray = new TickMask<bool>[_ticksPerMeasure + 1];
            for (int i = 1; i <= _ticksPerMeasure; i++)
            {
                _tickMaskArray[i] = new TickMask<bool>();  //TODO: See if this impacts performance , used to use != & continue statements before triplets
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

        void BuildBeatMask()
        {
            //print("Build that mask: " + _thirtySecondCount);
            ThisBeatMask.Clear();
            ThisBeatMask[BeatValue.ThirtySecond] = true;
            if (_thirtySecondCount % 2 != 0) return;
            ThisBeatMask[BeatValue.Sixteenth] = true;
            if (_thirtySecondCount % 4 != 0) return;
            ThisBeatMask[BeatValue.Eighth] = true;
            if (_thirtySecondCount % 8 != 0) return;
            ThisBeatMask[BeatValue.Quarter] = true;
            if (_thirtySecondCount % 16 != 0) return;
            ThisBeatMask[BeatValue.Half] = true;
            if (_thirtySecondCount % 32 != 0) return;
            ThisBeatMask[BeatValue.Measure] = true;
        }

        /// <summary>
        /// Set a new _bpm.
        /// </summary>
        /// <param name="newBPM">New _bpm, can be int, float, or double</param>
        public void SetBPM(int newBPM)
        {
            double BPMdbl = (double)newBPM;
            InitializeBPM(BPMdbl);
        }

        /// <summary>
        /// Set a new _bpm.
        /// </summary>
        /// <param name="newBPM">New _bpm, can be int, float, or double</param>
        public void SetBPM(float newBPM)
        {
            double BPMdbl = (double)newBPM;
            InitializeBPM(BPMdbl);
        }

        /// <summary>
        /// Set a new _bpm.
        /// </summary>
        /// <param name="newBPM">New _bpm, can be int, float, or double</param>
        public void SetBPM(double NewBPM)
        {
            InitializeBPM((NewBPM));
        }

        /// <summary>
        /// Internal function to intitialize a new _bpm.
        /// </summary>
        /// <param name="newBPM">_bpm to be initialzed</param>
        void InitializeBPM(double NewBPM)
        {
            _initialized = false;
            ResetBeatCounts();
            //if (NewBPM > 210)
            //{
            //    NewBPM = NewBPM / 2;
            //    Debug.LogWarning("New _bpm set > 210. Clock loses stability above ~210. Dividing _bpm in half, using " + NewBPM + " instead.");
            //}
            BPM = NewBPM;


            //running = true;
            _secondsPerMeasure = (60 / BPM) * beatsPerMeasure;
            //print("seconds per measure " + _secondsPerMeasure);
            _ticksPerMeasure = TicksPerBeat * beatsPerMeasure;
            _tickCounter = _ticksPerMeasure;
            SetLengths();
            BuildBeatMaskArray();
            StartCoroutine(FirstBeat());
            latency = new double[latencyCalculationInterval];
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
        IEnumerator FirstBeat()
        {
            yield return new WaitForSeconds(0.25f);
            Running = false;
            double startTick = AudioSettings.dspTime + StartDelay;
            _measureCount = 0;
            _startTime = startTick;
            Time = AudioSettings.dspTime - startTick;
            TimeMS = Time * 1000;

            //attempting to adjust TimeMS
            timeMSBuffer[0] = TimeMS;
            _msBufferIndex = 1;

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

            //Play an AudioSource on the same game object to be synced up with the clock
            //example - a metronome
            if (GetComponent<AudioSource>())
            {
                GetComponent<AudioSource>().PlayScheduled(startTick);
            }
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
            Source = GetComponent<AudioSource>();
            if (!BPM.Equals(0.0))
            {
                InitializeBPM(BPM);
            }
        }

        private bool init;

        public bool TimeLineControlled;
        //public PlayableDirector Director;

        void OnBehaviourPlay()
        {
            print("PLAY");
        }

        void OnBehaviourPause()
        {
            print("Pause");
        }

        public int ticksLastFrame;

        void Update()
        {
            if (!_initialized) return;
            if (TimeLineControlled)
            {
                //Time = Director.time;
                MBT newMBT = TimeToMBT((float)Time);

                Measures = newMBT.Measure;
                Beats = newMBT.Beat;
                Ticks = newMBT.Tick;
                _measureCount = newMBT.Measure;
                int totalTicks = (int)(Time / TickLength);
                if (ticksLastFrame != totalTicks)
                {
                    UpdateBeats();
                    BuildBeatMask();
                    ticksLastFrame = totalTicks;
                }
            }
            else
            {
                Time = AudioSettings.dspTime - _startTime;
                if (_thirtySecondCount != _lastThirtySecondCount)
                {
                    BuildBeatMask();
                    _lastThirtySecondCount = _thirtySecondCount;
                }
            }
            TimeMS = Time * 1000;

            timeMSBuffer[_msBufferIndex % timeMSBuffer.Length] = TimeMS;
            //slight adjusting for frame-audio buffer de sync
            if(_msBufferIndex > 1 && TimeMS == timeMSBuffer[(_msBufferIndex - 1) % timeMSBuffer.Length])
            {
                //check one prior frame
                int numDuplicates = 1;
                if(_msBufferIndex > 2 && TimeMS == timeMSBuffer[(_msBufferIndex - 2) % timeMSBuffer.Length])
                {
                    numDuplicates = 2;
                }
                //Debug.Log("duplicate time");
                double timeMSAdjusted = TimeMS + (double)(UnityEngine.Time.deltaTime * numDuplicates);
                //Debug.Log("TimeMS: " + TimeMS);

            } else
            {
                //Debug.Log("not duplicate time");
            }
            _msBufferIndex += 1;

            if (TimeMS > 0 && !init)
            {
                init = true;
                Running = true;
            }

        }

        public MBT GetMBT()
        {
            return new MBT(Measures, Beats, Ticks);
        }



        /// <summary>
        /// Using OnAudioFilter Read to updates beats & dispatches events accordingly.
        /// </summary>
        void OnAudioFilterRead(float[] data, int channels)
        {
            if (TimeLineControlled) return;
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
                    UpdateBeats();
                    Measures = _measureCount;
                    Beats = 1 + _tickCounter / (TicksPerBeat);
                    Ticks = 1 + _tickCounter % (TicksPerBeat);
                    //latency[_latencyCounter] = (_dspTimeAtTick - (_nextThirtySecond)); //benchmarking stuff
                }
                n++;
            }
            //_latencyCounter++;
            //if (_latencyCounter == latencyCalculationInterval)
            //{
            //    CalculateLatency();
            //}
        }

        /// <summary>
        /// Function to update beat counts, called after every new tick
        /// </summary>
        void UpdateBeats()
        {

            if (AudioUpdate != null) AudioUpdate();
            TickMask = _tickMaskArray[_tickCounter];
            if (TickMask[TickValue.ThirtySecond])
            {
                if (ThirtySecond != null)
                    ThirtySecond(new Args(TickValue.ThirtySecond, _nextThirtySecond,
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
                    Sixteenth(new Args(TickValue.Sixteenth, _nextSixteenth,
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
                    Eighth(new Args(TickValue.Eighth, _nextEighth,
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
                    Beat(new Args(TickValue.Quarter, _nextQuarter,
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
                    Measure(new Args(TickValue.Measure, _nextMeasure,
                        _nextMeasure + _measureLength, TickMask));
                _nextMeasure += _measureLength; 
                //_thirtySecondCount = 0;
            }
        }

        //Note - these sync functions do not work properly

        /// <summary>
        /// Sync the execution of a function to the next specified beat, optionally add a number repetitions
        /// </summary>
        /// <param name="callback">Function to call at next beatTickValue</param>
        /// <param name="beatTickValue">Beat value at which function will be called (Default is measure / downbeat)</param>
        public void SyncFunction(System.Action callback, TickValue beatTickValue = TickValue.Measure)
        {
            StartCoroutine(YieldForSync(callback, beatTickValue));
        }

        /// <summary>
        /// Sync the execution of a function to the next specified beat, optionally add a number repetitions
        /// </summary>
        /// <param name="callback">Function to call at next beatTickValue</param>
        /// <param name="beatTickValue">Beat value at which function will be called (Default is measure / downbeat)</param>
        IEnumerator YieldForSync(System.Action callback, TickValue beatTickValue)
        {
            return YieldForSyncRepeating(callback, beatTickValue, 1);
        }

        /// <summary>
        /// Sync the execution of a particular callback to a specified beat a specified number of times
        /// </summary>
        /// <param name="callback">Function to call at next beatTickValue</param>
        /// <param name="beatTickValue">Beat value at which function will be called</param>
        /// <param name="repetitions">Number of times function will be called</param>
        public void SyncFunction(System.Action callback, TickValue beatTickValue, int repetitions)
        {
            StartCoroutine(YieldForSyncRepeating(callback, beatTickValue, repetitions));
        }

        /// <summary>
        /// Sync the execution of a particular callback to a specified beat a specified number of times
        /// </summary>
        /// <param name="callback">Function to call</param>
        /// <param name="beatTickValue">Beat value at which function will be called</param>
        /// <param name="repetitions">Number of times function will be called</param>
        /// <returns></returns>
        IEnumerator YieldForSyncRepeating(System.Action callback, TickValue beatTickValue, int repetitions)
        {
            Debug.Log("Yield for sync TickMask: " + TickMask[beatTickValue]);
            int startCount = _tickCounter + _measureCount * _ticksPerMeasure;
            bool isStartNote = true;
            bool waiting = true;
            int timesRun = 0;
            int tickOfLastRun = 0;
            while (waiting)
            {
               
                isStartNote = isStartNote && startCount == _tickCounter + _measureCount * _ticksPerMeasure;
                if (isStartNote)
                    yield return false;
                
                isStartNote = false;
                
                if ((beatTickValue == TickValue.ThirtySecond || TickMask[beatTickValue]) && (tickOfLastRun != _tickCounter + _measureCount * _ticksPerMeasure))
                {
                    if (repetitions <= timesRun)
                    {
                        waiting = false;
                    }
                    else
                    {

                        callback();
                        tickOfLastRun = _tickCounter + _measureCount * _ticksPerMeasure;
                        timesRun++;
                        yield return false;
                    }
                }
                else
                    yield return false;
            }
        }

        /// <summary>
        /// Returns time of next specified value Usage: AudioSource.PlayScheduled(AtNext(Beat.TickValue.Quarter))
        /// </summary>
        /// <param name="beatTickValue">TickValue for which to get time of next</param>
        /// <returns>Time of next specified value</returns>
        public double AtNext(TickValue beatTickValue)
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
        public double AtNextThirtySecond()
        {
            return _nextThirtySecond;
        }
        /// <summary>
        /// Time of next 16th note triplet. Usage: AudioSource.PlayScheduled(AtNextSixteenth())
        /// </summary>
        /// <returns>Time of next 16th note triplet as double</returns>
        public double AtNextSixteenthTriplet()
        {
            return _nextSixteenth;
        }
        /// <summary>
        /// Time of next 16th note. Usage: AudioSource.PlayScheduled(AtNextSixteenth())
        /// </summary>
        /// <returns>Time of next 16th note as double</returns>
        public double AtNextSixteenth()
        {
            return _nextSixteenth;
        }
        /// <summary>
        /// Time of next 8th note triplet. Usage: AudioSource.PlayScheduled(AtNextEighth())
        /// </summary>
        /// <returns>Time of next 8th note as double</returns>
        public double AtNextEighthTriplet()
        {
            return _nextEighthTriplet;
        }
        /// <summary>
        /// Time of next 8th note. Usage: AudioSource.PlayScheduled(AtNextEighth())
        /// </summary>
        /// <returns>Time of next 8th note as double</returns>
        public double AtNextEighth()
        {
            return _nextEighth;
        }
        /// <summary>
        /// Time of next 1/4 note triplet. Usage: AudioSource.PlayScheduled(AtNextQuarter())
        /// </summary>
        /// <returns>Time of next 1/4 note triplet as double</returns>
        public double AtNextQuarterTriplet()
        {
            return _nextQuarterTriplet;
        }
        /// <summary>
        /// Time of next 1/4 note. Usage: AudioSource.PlayScheduled(AtNextQuarter())
        /// </summary>
        /// <returns>Time of next 1/4 note as double</returns>
        public double AtNextQuarter()
        {
            //Debug.Log("Next Quarter:" + _nextQuarter);
            return _nextQuarter;
        }
        /// <summary>
        /// Time of next beat (1/4 note). Usage: AudioSource.PlayScheduled(AtNextBeat())
        /// </summary>
        /// <returns>Time of next beat (1/4 note) as double</returns>
        public double AtNextBeat()
        {
            return _nextQuarter;
        }
        /// <summary>
        /// Time of next 1/2 note. Usage: AudioSource.PlayScheduled(AtNextHalf())
        /// </summary>
        /// <returns>Time of next 1/2 note as double</returns>
        public double AtNextHalf()
        {
            return _nextHalf;
        }
        /// <summary>
        /// Time of next measure. Usage: AudioSource.PlayScheduled(AtNextMeasure())
        /// </summary>
        /// <returns>Time of next measure as double</returns>
        public double AtNextMeasure()
        {
            return _nextMeasure;
        }

        /// <summary>
        /// Time of and MBT (Measure-Beat-Tick). Usage: AudioSource.PlayScheduled(AtMBT(mbt))
        /// </summary>
        /// <param name="mbt"></param>
        /// <returns></returns>
        public double AtMBT(MBT mbt)
        {
            double value = mbt.GetTickValue() * (Clock.Instance.SamplesPerTick / AudioSettings.outputSampleRate) + _startTime;
            return value;
        }

        /// <summary>
        /// Helper fucntions for timing things like tweens & animations to the beat clock
        /// </summary>
        /// <param name="beatTickValue"></param>
        /// <returns></returns>
        public float LengthOf(TickValue beatTickValue)
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

        public float ThirtySecondLength()
        {
            return (float)_thirtySecondLength;
        }

        public float SixteenthTripletLength()
        {
            return (float)_sixteenthTripletLength;
        }

        public float SixteenthLength()
        {
            return (float)_sixteenthLength;
        }

        public float EighthTripletLength()
        {
            return (float)_eighthTripletLength;
        }

        public float EighthLength()
        {
            return (float)_eighthLength;
        }

        public float QuarterTripletLength()
        {
            return (float)_quarterTripletLength;
        }

        public float QuarterLength()
        {
            return (float)_quarterLength;
        }

        public float BeatLength()
        {
            return (float)_quarterLength;
        }

        public float HalfLength()
        {
            return (float)_halfLength;
        }

        public float MeasureLength()
        {
            return (float)_measureLength;
        }

        /// <summary>
        /// Helper functions for timing precise audio playback to the beatclock
        /// </summary>
        /// <param name="beatTickValue"></param>
        /// <returns>Returns length of the beat value</returns>
        public double LengthOfD(TickValue beatTickValue)
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

        public double ThirtySecondLengthD()
        {
            return _thirtySecondLength;
        }

        public double SixteenthTripletLengthD()
        {
            return _sixteenthTripletLength;
        }

        public double SixteenthLengthD()
        {
            return _sixteenthLength;
        }

        public double EighthTripletLengthD()
        {
            return _eighthTripletLength;
        }

        public double EighthLengthD()
        {
            return _eighthLength;
        }

        public double QuarterTripletLengthD()
        {
            return _quarterTripletLength;
        }

        public double QuarterLengthD()
        {
            return _quarterLength;
        }

        public double BeatLengthD()
        {
            return _quarterLength;
        }

        public double HalfLengthD()
        {
            return _halfLength;
        }

        public double MeasureLengthD()
        {
            return _measureLength;
        }

        public double SampleRate()
        {
            return _sampleRate;
        }

        public MBT TickToMBT(int tick)
        {
            MBT newMBT = new MBT(0, 0, 0);
            if (tick == 0) return newMBT;
            newMBT.Measure = 1 + tick / (TicksPerBeat * 4);
            tick = tick - (newMBT.Measure - 1) * TicksPerBeat * 4;
            newMBT.Beat = 1 + tick / TicksPerBeat;
            newMBT.Tick = (tick % TicksPerBeat) / 10;
            return newMBT;
        }

        public float MidiTimeToTime(int tick, float BPM, int PPQ)
        {
            return tick * (60000 / (BPM * PPQ));
        }

        public MBT TimeToMBT(float time)
        {
            return TickToMBT((int)(time / TickLength));
        }

        /// <summary>
        /// Benchmarking function for latency compensation
        /// </summary>
        void CalculateLatency()
        {
            _rollingAverageOfLatency = Average(latency);
            _latencyCompensation += _rollingAverageOfLatency;
            _latencyCounter = 0;
            _standardDeviationOfLatency = StdDev(latency);
            AverageLatency = (int)(1000 * (_rollingAverageOfLatency));
            AverageJitter = (int)(1000 * (_standardDeviationOfLatency));
        }

        /// <summary>
        /// Benchmarking function for latency compensation
        /// </summary>
        /// <param name="values">List of values to be averaged</param>
        /// <returns>Average of list of values</returns>
        double Average(double[] values)
        {
            int periodLength = 0;
            double total = 0.0;
            foreach (double value in values)
            {
                total += value;
                periodLength++;
            }
            return total / periodLength;
        }

        /// <summary>
        /// Benchmarking function for latency compensation
        /// </summary>
        /// <param name="values">List of values for which to calculate standard deviation</param>
        /// <returns>Standard deviation of list of values</returns>
        double StdDev(double[] values)
        {
            double mean = 0.0;
            double sum = 0.0;
            double stdDev = 0.0;
            int periodLength = latencyCalculationInterval;
            foreach (double value in values)
            {
                double delta = value - mean;
                mean += delta / periodLength;
                sum += delta * (value - mean);
            }
            stdDev = Math.Sqrt(sum / (periodLength - 1));
            return stdDev;
        }

    }
}