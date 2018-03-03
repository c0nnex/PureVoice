using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamSpeakPlugin
{
    public enum ConnectionTarget : int
    {
        /// <summary>
        /// Connection in new tab.
        /// </summary>
        NEW_TAB = 0,

        /// <summary>
        /// Connect in current tab.
        /// </summary>
        CURRENT_TAB = 1,

        /// <summary>
        /// Connect new tab if current tab is connected. If not, connect in current tab.
        /// </summary>
        NEW_TAB_IF_CURRENT_USED = 2
    }
}
