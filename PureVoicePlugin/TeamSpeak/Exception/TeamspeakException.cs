using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TeamSpeakPlugin.Exceptions
{
    public class TeamspeakException : Exception
    {
        /// <summary>
        /// Throws an error.
        /// </summary>
        /// <param name="ErrorMessage">The error message.</param>
        /// <param name="TeamspeakExceptionCode">The error message.</param>
        public TeamspeakException(string ErrorMessage, int TeamspeakExceptionCode) : base(ErrorMessage)
        {
            string TeampSpeakErrorMessage = string.Format("Error!\nAn error has occoured in TeamSpeakPlugin.\nErrorcode: {0}.\nError message: {1}", TeamspeakExceptionCode.ToString(), ErrorMessage);
            MessageBoxButtons Buttons = MessageBoxButtons.OK;
            var result = MessageBox.Show(TeampSpeakErrorMessage, "Error in API", Buttons);
        }


        #region [ ERRORS ]

        #region [ MESSAGE ERRORS 0x0nnn ]

        /// <summary>
        /// When the target mode not found.
        /// </summary>
        public static int TARGET_MODE_NOT_FOUND = 0x0001;

        /// <summary>
        /// When the message text not found.
        /// </summary>
        public static int MESSAGE_NOT_FOUND = 0x0002;

        #endregion [ MESSAGE ERRORS 0x0nnn ]

        #region [ USER ERRORS 0x1nnn ]

        /// <summary>
        /// When the user not found.
        /// </summary>
        public static int USER_NOT_FOUND = 0x1000;

        #endregion [ USER ERRORS 0x1nnn ]

        #endregion [ ERRORS ]
    }
}
