using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Monitor
{
    // get from https://stackoverflow.com/questions/8840926/asynchronously-getforegroundwindow-via-sendmessage-or-something
    class ForegroundTracker
    {
        // Delegate and imports from pinvoke.net:

        delegate void WinEventDelegate( IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime );

        [DllImport( "user32.dll" )]
        static extern IntPtr SetWinEventHook( uint eventMin, uint eventMax, IntPtr
           hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
           uint idThread, uint dwFlags );

        [DllImport( "user32.dll" )]
        static extern bool UnhookWinEvent( IntPtr hWinEventHook );

        [DllImport( "user32.dll", CharSet = CharSet.Auto )]
        static extern int GetWindowText( IntPtr hWnd, StringBuilder title, int size );

        [DllImport( "user32.dll" )]
        static extern IntPtr GetDesktopWindow();


        // Constants from winuser.h
        const uint EVENT_SYSTEM_FOREGROUND = 3;
        const uint WINEVENT_OUTOFCONTEXT = 0;
        static IntPtr m_DESKTOP;
        static DebugLogger m_logger = new DebugLogger( "log.txt" );

        // Need to ensure delegate is not collected while we're using it,
        // storing it in a class field is simplest way to do this.
        static WinEventDelegate procDelegate = new WinEventDelegate( WinEventProc );

        public static void Start()
        {
            // Listen for foreground changes across all processes/threads on current desktop...
            IntPtr hhook = SetWinEventHook( EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero,
                    procDelegate, 0, 0, WINEVENT_OUTOFCONTEXT );

            m_DESKTOP = GetDesktopWindow();
            Console.WriteLine( $"DESKTP ptr {m_DESKTOP.ToInt64().ToString( "X" )}" );
            // MessageBox provides the necessary mesage loop that SetWinEventHook requires.
            MessageBox.Show( "Tracking focus, close message box to exit." );

            UnhookWinEvent( hhook );
        }

        static void WinEventProc( IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime )
        {
            StringBuilder title = new StringBuilder( 256 );
            GetWindowText( hwnd, title, 256 );
            //if( hwnd == m_DESKTOP ) {
            //    title.Clear();
            //    title.Append( "DESKTOP" );
            //}
            string szMsg = $"Foreground changed to {title} {hwnd.ToInt64().ToString( "X" )}";
            m_logger.WriteMsg( szMsg );
            Console.WriteLine( szMsg );
        }
    }
}