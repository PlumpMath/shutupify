﻿using Shutupify.Jukeboxes.Drivers;
using Shutupify.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Shutupify.Jukeboxes
{
    public class WebSocketController : IJukebox, IDisposable, ISettable
    {
        WebSocket _socket;
        Dictionary<JukeboxCommand, string> _messageMapping;

        public WebSocketController() 
        {
            _messageMapping = new Dictionary<JukeboxCommand, string>();
            _messageMapping.Add(JukeboxCommand.Play, "PLAY!");
            _messageMapping.Add(JukeboxCommand.Pause, "PAUSE!");
            //_messageMapping.Add(JukeboxCommand.NextTrack, "NextTrack");
            //_messageMapping.Add(JukeboxCommand.PreviousTrack, "PreviousTrack");
            //_messageMapping.Add(JukeboxCommand.Toggle, "PlayPause");
            _messageMapping.Add(JukeboxCommand.PlayAfterPaused, "PLAY!");
        }

        public WebSocketController(WebSocket socket) : this()
        {
            _socket = socket;
        }

        public bool IsAvailable
        {
            get {
                return IsActive && _socket.ClientConnected;
            }
        }

        public bool IsPlaying
        {
            get { return IsActive && _socket.IsPlaying(); }
        }


        public bool IsActive
        {
            get {
                return _socket != null;
            }
            set {
                if (value && _socket == null)
                    _socket = new WebSocket(_port, _chromeExtension);

                if (!value && _socket != null)
                {
                    _socket.Dispose();
                    _socket = null;
                }
            }
        }

        public void PerformAction(JukeboxCommand cmd)
        {
            if (!_messageMapping.ContainsKey(cmd))
                return;

            _socket.SendMessage(_messageMapping[cmd]);
        }

        public string Name
        {
            get { return "WebSocket"; }
        }

        public void ReadSettings(ISettingsReader settings)
        {
            settings.EnsureKey(this.Name + ":Port", "9971");
            settings.EnsureKey(this.Name + ":ChromeExtension", "mnkmaflojambglihddgpalgbfmogokfd");

            int port = 0;
            if (!int.TryParse(settings[this.Name + ":Port"], out port))
                port = 9971;
            _port = port;

            _chromeExtension = settings[this.Name + ":ChromeExtension"];
        }

        #region IDisposable
        private bool disposed = false;
        private string _chromeExtension;
        private int _port;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (_socket != null)
                        _socket.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose() // Implement IDisposable
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WebSocketController() // the finalizer
        {
            Dispose(false);
        }
        #endregion
    }
}
