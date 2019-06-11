using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AudioPPM;
using System.Diagnostics;
using NAudio.CoreAudioApi;
using System.IO;
using System.Management;
using System.Drawing.Imaging;
using System.Collections;
using System.Threading;
using Microsoft.DirectX.DirectInput;

namespace ControlGUI
{
public partial class Form1 : Form
{
    PpmGenerator _generator;
    private Joystick joystick;
    private const int CHANNELS_COUNT = 4;
    readonly byte[] _channelValues;
    IList<MMDevice> _devices;
    private bool _isPlaying;
    private int _countdownValue;
    private TimeSpan _countdownClock;
    private int _countdownStartValue;

    public Form1()
    {
        InitializeComponent();

        joystick = new Joystick(this.Handle);
        connectToJoystick(joystick);


        SetPlaying(false);
        _channelValues = new byte[CHANNELS_COUNT];
        UpdateDevices();

        this.FormClosing += Form1_FormClosing;
        trackbarThrottle.ValueChanged += Trackbar_ValueChanged;
        trackbarRudder.ValueChanged += Trackbar_ValueChanged;
        trackbarElevator.ValueChanged += Trackbar_ValueChanged;
        trackbarEleron.ValueChanged += Trackbar_ValueChanged;
           
            this.BringToFront();
            this.Focus();
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
            _countdownValue = (int) numericUpDown1.Value;


            _countdownStartValue = 300;
            numericUpDown1.Value = _countdownStartValue;
            _countdownClock = new TimeSpan(00,00,_countdownStartValue);
            countdownLabel.Text = _countdownClock.ToString();


        }

    #region GUI EVENTS

    private void Trackbar_ValueChanged(object sender, EventArgs e)
    {
        if (trackbarThrottle.Value >= trackbarElevator.Value)
        {
            trackbarThrottle.Value = trackbarElevator.Value;
                

        }

        label1.Text = trackbarThrottle.Value.ToString();
        label2.Text = trackbarRudder.Value.ToString();
        label4.Text = trackbarElevator.Value.ToString();
        UpdateValues();
    }





    // Start PPM Generator
    private void btnStart_Click(object sender, EventArgs e)
    {
        if (_isPlaying)
            return;

        if(listboxDevices.SelectedIndex == -1)
        {
            MessageBox.Show("Please, select the output device", "PPM Control", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
             countdownLabel.Text = _countdownClock.ToString();
            startShutdownTimer();
       //     label5.Text = "Countdown ACTIVE!";
        _generator = new PpmGenerator(CHANNELS_COUNT, StandartProfiles.FlySky, _devices[listboxDevices.SelectedIndex]);
        _generator.SetValues(_channelValues);
        _generator.Start();
        SetPlaying(true);

          
    }

        private void startShutdownTimer()
        {
            _countdownStartValue = (int)numericUpDown1.Value;
            shutdownTimer.Start();
            numericUpDown1.Enabled = false;
        }

        private void stopShutdownTimer()
        {
            shutdownTimer.Stop();
            _countdownStartValue = (int)numericUpDown1.Value;
            // _countdownValue = (int)numericUpDown1.Value;
            _generator?.Stop();
            SetPlaying(false);
            numericUpDown1.Enabled = true;
        }

    // Stop PPM Generator
    private void btnStop_Click(object sender, EventArgs e)
    {
        _generator?.Stop();
        SetPlaying(false);
        stopShutdownTimer();
    }

    private void btnUpdate_Click(object sender, EventArgs e)
    {
        UpdateDevices();
    }


    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        _generator?.Stop();
    }

    private void Form1_KeyPress(object sender, KeyEventArgs e)
    {
        if(e.KeyCode == Keys.Space)
        {
            joystickTimer.Stop();
            MessageBox.Show("Emergency stop!");
        }
    }
        
    #endregion


    // Get values from trackbars and set them to PPM Generator
    private void UpdateValues()
    {
        //  _channelValues[0] = (byte)trackbarEleron.Value;
        //  _channelValues[1] = (byte)trackbarElevator.Value;
        _channelValues[2] = (byte)trackbarThrottle.Value;
        _channelValues[3] = (byte)trackbarRudder.Value;

        _generator.SetValues(_channelValues);
    }


    // Update list of output devices
    private void UpdateDevices()
    {
        listboxDevices.Items.Clear();
        _devices = PpmGenerator.GetDevices().OrderBy(x=>x.FriendlyName).ToList();

        foreach (var device in _devices)
            listboxDevices.Items.Add($"{device.FriendlyName} - {device.State}");
    }


    // Set playing state and GUI
    private void SetPlaying(bool isPlaying)
    {
        _isPlaying = isPlaying;

        if (isPlaying)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;
        }
        else
        {
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }
    }


    private void connectToJoystick(Joystick joystick)
    {
        while (true)
        {
            string sticks = joystick.FindJoysticks();
            if (sticks != null)
            {
                if (joystick.AcquireJoystick(sticks))
                {
                    enableTimer();
                    break;
                }
            }
        }
    }

    private void enableTimer()
    {
        if (this.InvokeRequired)
        {
            BeginInvoke(new ThreadStart(delegate ()
            {
                joystickTimer.Enabled = true;
            }));
        }
        else
        {
            joystickTimer.Enabled = true;
        }
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        joystickTimer.Enabled = true;
    }


    private void joystickTimer_Tick_1(object sender, EventArgs e)
    {

        label1.Text = joystick.Yaxis.ToString();
       
        try
        {
            joystick.UpdateStatus();
                                                           
                               
        }
        catch
        {
            joystickTimer.Enabled = false;
            connectToJoystick(joystick);
        }
    }

    private void JoystickTimer_Tick(object sender, EventArgs e)
    {
            
        try
        {
            joystick.UpdateStatus();
                

            trackbarRudder.Value = (joystick.Xaxis-20) / 256;
            trackbarThrottle.Value = 255-((65536-joystick.Zaxis+joystick.Yaxis) / 512);
                
                
            //UpdateValues();

        }
        catch
        {
            joystickTimer.Enabled = false;
            connectToJoystick(joystick);
        }
    }

    private void Label3_Click(object sender, EventArgs e)
    {

    }

    private void Form1_KeyDown(object sender, KeyEventArgs e)
    {
            if(e.KeyCode == Keys.Space)
            {
                _generator?.Stop();
                SetPlaying(false);
                stopShutdownTimer();
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void shutdownTimer_Tick(object sender, EventArgs e)
        {
            _countdownClock = _countdownClock.Subtract(new TimeSpan(0, 0, 1));
            countdownLabel.Text = _countdownClock.ToString();

            if (_countdownClock.TotalSeconds == 0)
            {
                stopShutdownTimer();
                _countdownClock = new TimeSpan(0, 0, _countdownStartValue);
              //  countdownLabel.Text = _countdownClock.ToString();

            }

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            _countdownStartValue = (int) numericUpDown1.Value;
            _countdownClock = new TimeSpan(0, 0, _countdownStartValue);
            countdownLabel.Text = _countdownClock.ToString();

        }
    }
}
