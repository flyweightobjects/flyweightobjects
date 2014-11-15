//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
//  Author:         Marcus Crane, Software Developer / Architect                                        //
//  E-mail:         mailto:support@FlyweightObjects.NET                                                 //
//  Company:        FlyweightObjects.NET                                                                //
//  Copyright:      Copyright © FlyweightObjects.NET 2011, All rights reserved.                         //
//  Date Created:   06/04/2008                                                                          //
//                                                                                                      //
//  Disclaimer:                                                                                         //
//  ===========                                                                                         //
//  This code file is provided "as is" with no expressed or implied warranty. The author accepts no     //
//  liability for any damage or loss that the code file may cause as a result of its use. Any           //
//  modification, copying, or reverse engineering of this code file, or the underlying architectural    //
//  foundation it supports, is strictly prohibited without the express written consent of the author.   //
//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using System.Diagnostics;
using System.ServiceProcess;

namespace FlyweightObjects
{
	/// <summary>
	/// The channel type used to construct a new ServiceListener.
	/// </summary>
	public enum ChannelType
	{
		/// <summary>
		/// A TCP remoting channel.
		/// </summary>
		Tcp,
		/// <summary>
		/// An HTTP remoting channel.
		/// </summary>
		Http
	}

	/// <summary>
	/// Provides a windows service implementation of a remoting listener service.
	/// </summary>
	public abstract class ServiceListenerBase : ServiceBase
	{
		private readonly ChannelType _channelType = ChannelType.Tcp;
		private IChannelReceiver _serverChannel = null;
		private readonly int _port = 0;
		private readonly Type _remotedType = null;
		private readonly bool _secureChannel = false;
		private readonly WellKnownObjectMode _remotedObjectMode = WellKnownObjectMode.SingleCall;
		private readonly CustomErrorsModes _customErrorModes = CustomErrorsModes.Off;
		private string _applicationName = string.Empty;
		
		/// <summary>
		/// Gets the ChannelType used by the service.
		/// </summary>
		public ChannelType ChannelType
		{
			get { return _channelType; }
		}

		/// <summary>
		/// Gets the IChannel used by the service.
		/// </summary>
		public IChannel ServerChannel
		{
			get { return _serverChannel; }
		}

		/// <summary>
		/// Gets the port used by the service.
		/// </summary>
		public int Port
		{
			get { return _port; }
		}

		/// <summary>
		/// Gets the MarshalByRefObject type remoted by the service.
		/// </summary>
		public Type RemotedType
		{
			get { return _remotedType; }
		}

		/// <summary>
		/// Gets whether or not to use a secure channel for the service.
		/// </summary>
		public bool SecureChannel
		{
			get { return _secureChannel; }
		}

		/// <summary>
		/// Gets the WellKnownObjectMode for the service's remoted type(s).
		/// </summary>
		public WellKnownObjectMode RemotedObjectMode
		{
			get { return _remotedObjectMode; }
		}

		/// <summary>
		/// Gets the CustomErrorsModes property for the remoting configuration.
		/// </summary>
		public CustomErrorsModes CustomErrorsModes
		{
			get { return _customErrorModes; }
		}

		/// <summary>
		/// Gets the application name for the instance of the class.
		/// </summary>
		public string ApplicationName
		{
			get { return _applicationName; }
		}

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            base.EventLog.WriteEntry(e.ExceptionObject.ToString(), EventLogEntryType.Error);
        }
        
        /// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="remotedType">The MarshalByRefObject type to remote.</param>
		/// <param name="channelType">The type of channel to use.</param>
		/// <param name="port">The port number to open.</param>
		public ServiceListenerBase(Type remotedType, ChannelType channelType, int port)
		{
            InitializeComponent();
            ValidateRemotedType(remotedType);
			_remotedType = remotedType;
			_channelType = channelType;
			_port = port;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="channelType">The type of channel to use.</param>
		/// <param name="port">The port number to open.</param>
		/// <param name="remotedType">The MarshalByRefObject type to remote.</param>
		/// <param name="secureChannel">Determines whether or not to use a secure channel.</param>
		public ServiceListenerBase(Type remotedType, ChannelType channelType, int port, bool secureChannel)
            : this(remotedType, channelType, port)
		{
			_secureChannel = secureChannel;
		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="channelType">The type of channel to use.</param>
		/// <param name="port">The port number to open.</param>
		/// <param name="remotedType">The MarshalByRefObject type to remote.</param>
		/// <param name="secureChannel">Determines whether or not to use a secure channel.</param>
		/// <param name="customErrorModes">Sets the CustomErrorModes for the RemotingConfiguration.</param>
		public ServiceListenerBase(Type remotedType, ChannelType channelType, int port, bool secureChannel, CustomErrorsModes customErrorModes)
            : this(remotedType, channelType, port)
		{
			_secureChannel = secureChannel;
			_customErrorModes = customErrorModes;
		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="remotedType">The MarshalByRefObject type to remote.</param>
		/// <param name="channelType">The type of channel to use.</param>
		/// <param name="port">The port number to open.</param>
		/// <param name="secureChannel">Determines whether or not to use a secure channel.</param>
		/// <param name="remotedObjectMode">The mode (Singleton or SingleCall) for the remoted type.</param>
		public ServiceListenerBase(Type remotedType, ChannelType channelType, int port, bool secureChannel, WellKnownObjectMode remotedObjectMode)
            : this(remotedType, channelType, port)
		{
			_secureChannel = secureChannel;
			_remotedObjectMode = remotedObjectMode;
		}

		/// <summary>
		/// Starts the service.
		/// </summary>
		public void Start()
		{
			OnStart(new string[0]);
		}

		/// <summary>
		/// Starts the service.
		/// </summary>
		/// <param name="args">Arguments for the service.</param>
		protected override void OnStart(string[] args)
		{
			try
			{
				_serverChannel = _channelType == ChannelType.Http ? new HttpServerChannel(Guid.NewGuid().ToString(), _port, new BinaryServerFormatterSinkProvider()) as IChannelReceiver 
					: new TcpServerChannel(Guid.NewGuid().ToString(), _port, new BinaryServerFormatterSinkProvider()) as IChannelReceiver;
				
				ChannelServices.RegisterChannel(_serverChannel, _secureChannel);
				if (!string.IsNullOrEmpty(_applicationName))
				{
					RemotingConfiguration.ApplicationName = _applicationName;
				}
				RemotingConfiguration.RegisterWellKnownServiceType(_remotedType, _remotedType.Name, _remotedObjectMode);
                try 
                { 
                    RemotingConfiguration.CustomErrorsMode = _customErrorModes; 
                }
                catch (Exception e) 
                { 
                    base.EventLog.WriteEntry(e.ToString(), EventLogEntryType.Error); 
                }
			}
			catch (Exception e)
			{
				if (!Debugger.IsAttached)
				{
					base.EventLog.WriteEntry(e.ToString(), EventLogEntryType.Error);
				}
				else
				{
					throw;
				}
			}
		}

		/// <summary>
		/// Stops the service.
		/// </summary>
		protected override void OnStop()
		{
			try
			{
				if (_serverChannel != null)
				{
					ChannelServices.UnregisterChannel(_serverChannel);
				}
			}
			catch (Exception e)
			{
				if (!Debugger.IsAttached)
				{
					base.EventLog.WriteEntry(e.ToString(), EventLogEntryType.Error);
				}
				else
				{
					throw e;
				}
			}
		}

		private void ValidateRemotedType(Type remotedType)
		{
			if (!typeof(MarshalByRefObject).IsAssignableFrom(remotedType))
			{
				ArgumentException e = new ArgumentException(string.Format("The type \"{0}\" specified in the arguments does not implement MarshalByRefObject.", remotedType.Name));
				if (!Debugger.IsAttached)
				{
					base.EventLog.WriteEntry(e.ToString(), EventLogEntryType.Error);
				}
				else
				{
					throw e;
				}
			}
		}

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.ServiceName = "RemotingListenerService";
		}

		#endregion

	}
}
