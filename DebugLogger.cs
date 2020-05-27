using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Monitor
{
	public class DebugLogger
	{
		public DebugLogger( string szPath )
		{
			// Move file first then set path
			MoveTooLargeLogFileToBackup( szPath );
			m_szPath = szPath;
		}

		public void WriteMsg( string szMsg )
		{
			try {
				// Incase different thread getting same file.
				string szTimestamp = DateTime.Now.ToString( "yyyy/MM/dd HH:mm:ss fff" );
				string szFullMsg = szTimestamp + " " + szMsg;
				System.IO.StreamWriter sw = new System.IO.StreamWriter( m_szPath, true );
				sw.WriteLine( szFullMsg );
				sw.Flush();
				sw.Close();
				System.Diagnostics.Debug.WriteLine( szFullMsg );
			}
			catch( Exception ex ) {
				System.Diagnostics.Debug.WriteLine( "DebugLogger:" + m_szPath + "\r\n" + ex.ToString() );
			}
		}

		public static void ShowDebugMsg( string szMsg )
		{
			string szTimestamp = DateTime.Now.ToString( "yyyy/MM/dd HH:mm:ss fff" );
			string szFullMsg = szTimestamp + " " + szMsg;
			System.Diagnostics.Debug.WriteLine( szFullMsg );
		}

		public static void MoveTooLargeLogFileToBackup( string szFilePath )
		{
			FileInfo file = new FileInfo( szFilePath );
			if( file.Exists == false ) {
				return;
			}
			// If file is larger than ~10mb, move it to backup
			bool bFileTooLarge = file.Length > SZFileBackupSize;
			if( bFileTooLarge == false ) {
				return;
			}
			// Copy file as backup
			try {
				File.Copy( szFilePath, szFilePath + SZLogFileBackupSuffix, true );
			}
			catch {
				// skip errors
			}
			// Remove original file
			try {
				File.Delete( szFilePath );
			}
			catch {
				// skip errors
			}
		}

		// ================= private ====================
		const string SZLogFileBackupSuffix = ".bk";

		// If size > 10mb, move to backup
		const long SZFileBackupSize = (long)1E6 * 10;



		string m_szPath;
	}
}
