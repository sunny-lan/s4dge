using System;
using UnityEngine;

public static class Log {
    public class LogTag {
        public bool isEnabled { get; private set; } = false;
        public LogTag( bool _isEnabled ) {
            isEnabled = _isEnabled;
        }
    };

    // List all new tags here so that they can be enabled / disabled when testing
    public static LogTag collisions = new LogTag( false ); // EX: set this to true to enable all collision related prints
    public static LogTag meshRendering = new LogTag( true );

    /// <summary>
    /// Prints the given message if the associated log tag is enabled
    /// </summary>
    public static void Print( string message, LogTag tag ) {
        if ( tag.isEnabled ) {
            Debug.Log( message );
        }
    }
}