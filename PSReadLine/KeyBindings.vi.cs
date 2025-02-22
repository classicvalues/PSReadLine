﻿/********************************************************************++
Copyright (c) Microsoft Corporation.  All rights reserved.
--********************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell
{
    public partial class PSConsoleReadLine
    {
        internal static ConsoleColor AlternateBackground(ConsoleColor bg)
        {
            switch (bg)
            {
                case ConsoleColor.Black: return ConsoleColor.DarkGray;
                case ConsoleColor.Blue: return ConsoleColor.DarkBlue;
                case ConsoleColor.Cyan: return ConsoleColor.DarkCyan;
                case ConsoleColor.DarkBlue: return ConsoleColor.Black;
                case ConsoleColor.DarkCyan: return ConsoleColor.Black;
                case ConsoleColor.DarkGray: return ConsoleColor.Black;
                case ConsoleColor.DarkGreen: return ConsoleColor.Black;
                case ConsoleColor.DarkMagenta: return ConsoleColor.Black;
                case ConsoleColor.DarkRed: return ConsoleColor.Black;
                case ConsoleColor.DarkYellow: return ConsoleColor.Black;
                case ConsoleColor.Gray: return ConsoleColor.White;
                case ConsoleColor.Green: return ConsoleColor.DarkGreen;
                case ConsoleColor.Magenta: return ConsoleColor.DarkMagenta;
                case ConsoleColor.Red: return ConsoleColor.DarkRed;
                case ConsoleColor.White: return ConsoleColor.Gray;
                case ConsoleColor.Yellow: return ConsoleColor.DarkYellow;
                default:
                    return ConsoleColor.Black;
            }
        }

        private int _normalCursorSize = 10;

        private static Dictionary<PSKeyInfo, KeyHandler> _viInsKeyMap;
        private static Dictionary<PSKeyInfo, KeyHandler> _viCmdKeyMap;
        private static Dictionary<PSKeyInfo, KeyHandler> _viChordDTable;
        private static Dictionary<PSKeyInfo, KeyHandler> _viChordGTable;
        private static Dictionary<PSKeyInfo, KeyHandler> _viChordCTable;
        private static Dictionary<PSKeyInfo, KeyHandler> _viChordYTable;
        private static Dictionary<PSKeyInfo, KeyHandler> _viChordDGTable;

        private static Dictionary<PSKeyInfo, Dictionary<PSKeyInfo, KeyHandler>> _viCmdChordTable;
        private static Dictionary<PSKeyInfo, Dictionary<PSKeyInfo, KeyHandler>> _viInsChordTable;

        /// <summary>
        /// Sets up the key bindings for vi operations.
        /// </summary>
        private void SetDefaultViBindings()
        {
            _viInsKeyMap = new Dictionary<PSKeyInfo, KeyHandler>
            {
                { Keys.Enter,           MakeKeyHandler(AcceptLine,             "AcceptLine" ) },
                { Keys.CtrlD,           MakeKeyHandler(ViAcceptLineOrExit,     "ViAcceptLineOrExit" ) },
                { Keys.ShiftEnter,      MakeKeyHandler(AddLine,                "AddLine") },
                { Keys.Escape,          MakeKeyHandler(ViCommandMode,          "ViCommandMode") },
                { Keys.LeftArrow,       MakeKeyHandler(ViBackwardChar,         "ViBackwardChar") },
                { Keys.RightArrow,      MakeKeyHandler(ViForwardChar,          "ViForwardChar") },
                { Keys.CtrlLeftArrow,   MakeKeyHandler(BackwardWord,           "BackwardWord") },
                { Keys.CtrlRightArrow,  MakeKeyHandler(NextWord,               "NextWord") },
                { Keys.UpArrow,         MakeKeyHandler(PreviousHistory,        "PreviousHistory") },
                { Keys.DownArrow,       MakeKeyHandler(NextHistory,            "NextHistory") },
                { Keys.Home,            MakeKeyHandler(BeginningOfLine,        "BeginningOfLine") },
                { Keys.End,             MakeKeyHandler(EndOfLine,              "EndOfLine") },
                { Keys.Delete,          MakeKeyHandler(DeleteChar,             "DeleteChar") },
                { Keys.Backspace,       MakeKeyHandler(BackwardDeleteChar,     "BackwardDeleteChar") },
                { Keys.CtrlSpace,       MakeKeyHandler(PossibleCompletions,    "PossibleCompletions") },
                { Keys.Tab,             MakeKeyHandler(ViTabCompleteNext,      "ViTabCompleteNext") },
                { Keys.ShiftTab,        MakeKeyHandler(ViTabCompletePrevious,  "ViTabCompletePrevious") },
                { Keys.CtrlU,           MakeKeyHandler(BackwardDeleteInput,    "BackwardDeleteInput") },
                { Keys.CtrlV,           MakeKeyHandler(Paste,                  "Paste") },
                { Keys.CtrlC,           MakeKeyHandler(CancelLine,             "CancelLine") },
                { Keys.CtrlL,           MakeKeyHandler(ClearScreen,            "ClearScreen") },
                { Keys.CtrlT,           MakeKeyHandler(SwapCharacters,         "SwapCharacters") },
                { Keys.CtrlY,           MakeKeyHandler(Redo,                   "Redo") },
                { Keys.CtrlZ,           MakeKeyHandler(Undo,                   "Undo") },
                { Keys.CtrlBackspace,   MakeKeyHandler(BackwardKillWord,       "BackwardKillWord") },
                { Keys.CtrlEnd,         MakeKeyHandler(ForwardDeleteInput,     "ForwardDeleteInput") },
                { Keys.CtrlHome,        MakeKeyHandler(BackwardDeleteInput,    "BackwardDeleteInput") },
                { Keys.CtrlRBracket,    MakeKeyHandler(GotoBrace,              "GotoBrace") },
                { Keys.F3,              MakeKeyHandler(CharacterSearch,        "CharacterSearch") },
                { Keys.ShiftF3,         MakeKeyHandler(CharacterSearchBackward,"CharacterSearchBackward") },
                { Keys.CtrlAltQuestion, MakeKeyHandler(ShowKeyBindings,        "ShowKeyBindings") },
                { Keys.CtrlR,           MakeKeyHandler(ReverseSearchHistory,   "ReverseSearchHistory") },
                { Keys.CtrlS,           MakeKeyHandler(ForwardSearchHistory,   "ForwardSearchHistory") },
                { Keys.CtrlG,           MakeKeyHandler(Abort,                  "Abort") },
                { Keys.AltH,            MakeKeyHandler(ShowParameterHelp,      "ShowParameterHelp") },
                { Keys.F1,              MakeKeyHandler(ShowCommandHelp,        "ShowCommandHelp") },
            };

            // Some bindings are not available on certain platforms
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _viInsKeyMap.Add(Keys.CtrlDelete, MakeKeyHandler(KillWord, "KillWord"));
            }

            _viCmdKeyMap = new Dictionary<PSKeyInfo, KeyHandler>
            {
                { Keys.Enter,           MakeKeyHandler(ViAcceptLine,         "ViAcceptLine") },
                { Keys.CtrlD,           MakeKeyHandler(ViAcceptLineOrExit,   "ViAcceptLineOrExit") },
                { Keys.ShiftEnter,      MakeKeyHandler(AddLine,              "AddLine") },
                { Keys.Escape,          MakeKeyHandler(Ding,                 "Ignore") },
                { Keys.LeftArrow,       MakeKeyHandler(ViBackwardChar,       "ViBackwardChar") },
                { Keys.RightArrow,      MakeKeyHandler(ViForwardChar,        "ViForwardChar") },
                { Keys.Space,           MakeKeyHandler(ViForwardChar,        "ViForwardChar") },
                { Keys.CtrlLeftArrow,   MakeKeyHandler(BackwardWord,         "BackwardWord") },
                { Keys.CtrlRightArrow,  MakeKeyHandler(NextWord,             "NextWord") },
                { Keys.UpArrow,         MakeKeyHandler(PreviousHistory,      "PreviousHistory") },
                { Keys.DownArrow,       MakeKeyHandler(NextHistory,          "NextHistory") },
                { Keys.Home,            MakeKeyHandler(BeginningOfLine,      "BeginningOfLine") },
                { Keys.End,             MakeKeyHandler(MoveToEndOfLine,      "MoveToEndOfLine") },
                { Keys.Delete,          MakeKeyHandler(DeleteChar,           "DeleteChar") },
                { Keys.Backspace,       MakeKeyHandler(ViBackwardChar,       "ViBackwardChar") },
                { Keys.CtrlSpace,       MakeKeyHandler(PossibleCompletions,  "PossibleCompletions") },
                { Keys.Tab,             MakeKeyHandler(TabCompleteNext,      "TabCompleteNext") },
                { Keys.ShiftTab,        MakeKeyHandler(TabCompletePrevious,  "TabCompletePrevious") },
                { Keys.CtrlV,           MakeKeyHandler(Paste,                "Paste") },
                { Keys.CtrlC,           MakeKeyHandler(CancelLine,           "CancelLine") },
                { Keys.CtrlL,           MakeKeyHandler(ClearScreen,          "ClearScreen") },
                { Keys.CtrlT,           MakeKeyHandler(SwapCharacters,       "SwapCharacters") },
                { Keys.CtrlU,           MakeKeyHandler(BackwardDeleteInput,  "BackwardDeleteInput") },
                { Keys.CtrlW,           MakeKeyHandler(BackwardDeleteWord,   "BackwardDeleteWord") },
                { Keys.CtrlY,           MakeKeyHandler(Redo,                 "Redo") },
                { Keys.CtrlZ,           MakeKeyHandler(Undo,                 "Undo") },
                { Keys.CtrlBackspace,   MakeKeyHandler(BackwardKillWord,     "BackwardKillWord") },
                { Keys.CtrlEnd,         MakeKeyHandler(ForwardDeleteInput,   "ForwardDeleteInput") },
                { Keys.CtrlHome,        MakeKeyHandler(BackwardDeleteInput,  "BackwardDeleteInput") },
                { Keys.CtrlRBracket,    MakeKeyHandler(GotoBrace,            "GotoBrace") },
                { Keys.F3,              MakeKeyHandler(CharacterSearch,      "CharacterSearch") },
                { Keys.ShiftF3,         MakeKeyHandler(CharacterSearchBackward, "CharacterSearchBackward") },
                { Keys.A,               MakeKeyHandler(ViInsertWithAppend,   "ViInsertWithAppend") },
                { Keys.B,               MakeKeyHandler(ViBackwardWord,       "ViBackwardWord") },
                { Keys.C,               MakeKeyHandler(ViChord,              "ChordFirstKey") },
                { Keys.D,               MakeKeyHandler(ViChord,              "ChordFirstKey") },
                { Keys.E,               MakeKeyHandler(NextWordEnd,          "NextWordEnd") },
                { Keys.F,               MakeKeyHandler(SearchChar,           "SearchChar") },
                { Keys.G,               MakeKeyHandler(ViChord,              "ChordFirstKey") },
                { Keys.H,               MakeKeyHandler(ViBackwardChar,       "ViBackwardChar") },
                { Keys.I,               MakeKeyHandler(ViInsertMode,         "ViInsertMode") },
                { Keys.J,               MakeKeyHandler(NextHistory,          "NextHistory") },
                { Keys.K,               MakeKeyHandler(PreviousHistory,      "PreviousHistory") },
                { Keys.L,               MakeKeyHandler(ViForwardChar,        "ViForwardChar") },
                { Keys.M,               MakeKeyHandler(Ding,                 "Ignore") },
                { Keys.N,               MakeKeyHandler(RepeatSearch,         "RepeatSearch") },
                { Keys.O,               MakeKeyHandler(ViAppendLine,         "ViAppendLine") },
                { Keys.P,               MakeKeyHandler(PasteAfter,           "PasteAfter") },
                { Keys.Q,               MakeKeyHandler(Ding,                 "Ignore") },
                { Keys.R,               MakeKeyHandler(ReplaceCharInPlace,   "ReplaceCharInPlace") },
                { Keys.S,               MakeKeyHandler(ViInsertWithDelete,   "ViInsertWithDelete") },
                { Keys.T,               MakeKeyHandler(SearchCharWithBackoff,"SearchCharWithBackoff") },
                { Keys.U,               MakeKeyHandler(Undo,                 "Undo") },
                { Keys.V,               MakeKeyHandler(ViEditVisually,       "ViEditVisually") },
                { Keys.W,               MakeKeyHandler(ViNextWord,           "ViNextWord") },
                { Keys.X,               MakeKeyHandler(DeleteChar,           "DeleteChar") },
                { Keys.Y,               MakeKeyHandler(ViChord,              "ChordFirstKey") },
                { Keys.Z,               MakeKeyHandler(Ding,                 "Ignore") },
                { Keys.ucA,             MakeKeyHandler(ViInsertAtEnd,        "ViInsertAtEnd") },
                { Keys.ucB,             MakeKeyHandler(ViBackwardGlob,       "ViBackwardGlob") },
                { Keys.ucC,             MakeKeyHandler(ViReplaceToEnd,       "ViReplaceToEnd") },
                { Keys.ucD,             MakeKeyHandler(DeleteToEnd,          "DeleteToEnd") },
                { Keys.ucE,             MakeKeyHandler(ViEndOfGlob,          "ViEndOfGlob") },
                { Keys.ucF,             MakeKeyHandler(SearchCharBackward,   "SearchCharBackward") },
                { Keys.ucG,             MakeKeyHandler(MoveToLastLine,       "MoveToLastLine") },
                { Keys.ucH,             MakeKeyHandler(Ding,                 "Ignore") },
                { Keys.ucI,             MakeKeyHandler(ViInsertAtBegining,   "ViInsertAtBegining") },
                { Keys.ucJ,             MakeKeyHandler(ViJoinLines,          "ViJoinLines") },
                { Keys.ucK,             MakeKeyHandler(Ding,                 "Ignore") },
                { Keys.ucL,             MakeKeyHandler(Ding,                 "Ignore") },
                { Keys.ucM,             MakeKeyHandler(Ding,                 "Ignore") },
                { Keys.ucN,             MakeKeyHandler(RepeatSearchBackward, "RepeatSearchBackward") },
                { Keys.ucO,             MakeKeyHandler(ViInsertLine,         "ViInsertLine") },
                { Keys.ucP,             MakeKeyHandler(PasteBefore,          "PasteBefore") },
                { Keys.ucQ,             MakeKeyHandler(Ding,                 "Ignore") },
                { Keys.ucR,             MakeKeyHandler(ViReplaceUntilEsc,    "ViReplaceUntilEsc") },
                { Keys.ucS,             MakeKeyHandler(ViReplaceLine,        "ViReplaceLine") },
                { Keys.ucT,             MakeKeyHandler(SearchCharBackwardWithBackoff, "SearchCharBackwardWithBackoff") },
                { Keys.ucU,             MakeKeyHandler(UndoAll,              "UndoAll") },
                { Keys.ucV,             MakeKeyHandler(Ding,                 "Ignore") },
                { Keys.ucW,             MakeKeyHandler(ViNextGlob,           "ViNextGlob") },
                { Keys.ucX,             MakeKeyHandler(BackwardDeleteChar,   "BackwardDeleteChar") },
                { Keys.ucY,             MakeKeyHandler(Ding,                 "Ignore") },
                { Keys.ucZ,             MakeKeyHandler(Ding,                 "Ignore") },
                { Keys._0,              MakeKeyHandler(DigitArgument,        "DigitArgument") },
                { Keys._1,              MakeKeyHandler(DigitArgument,        "DigitArgument") },
                { Keys._2,              MakeKeyHandler(DigitArgument,        "DigitArgument") },
                { Keys._3,              MakeKeyHandler(DigitArgument,        "DigitArgument") },
                { Keys._4,              MakeKeyHandler(DigitArgument,        "DigitArgument") },
                { Keys._5,              MakeKeyHandler(DigitArgument,        "DigitArgument") },
                { Keys._6,              MakeKeyHandler(DigitArgument,        "DigitArgument") },
                { Keys._7,              MakeKeyHandler(DigitArgument,        "DigitArgument") },
                { Keys._8,              MakeKeyHandler(DigitArgument,        "DigitArgument") },
                { Keys._9,              MakeKeyHandler(DigitArgument,        "DigitArgument") },
                { Keys.Dollar,          MakeKeyHandler(MoveToEndOfLine,      "MoveToEndOfLine") },
                { Keys.Percent,         MakeKeyHandler(ViGotoBrace,          "ViGotoBrace") },
                { Keys.Pound,           MakeKeyHandler(PrependAndAccept,     "PrependAndAccept") },
                { Keys.Pipe,            MakeKeyHandler(GotoColumn,           "GotoColumn") },
                { Keys.Uphat,           MakeKeyHandler(GotoFirstNonBlankOfLine, "GotoFirstNonBlankOfLine") },
                { Keys.Underbar,        MakeKeyHandler(GotoFirstNonBlankOfLine, "GotoFirstNonBlankOfLine") },
                { Keys.Tilde,           MakeKeyHandler(InvertCase,           "InvertCase") },
                { Keys.Slash,           MakeKeyHandler(ViSearchHistoryBackward, "ViSearchHistoryBackward") },
                { Keys.Question,        MakeKeyHandler(SearchForward,        "SearchForward") },
                { Keys.CtrlR,           MakeKeyHandler(ReverseSearchHistory, "ReverseSearchHistory") },
                { Keys.CtrlS,           MakeKeyHandler(ForwardSearchHistory, "ForwardSearchHistory") },
                { Keys.CtrlG,           MakeKeyHandler(Abort,                "Abort") },
                { Keys.Plus,            MakeKeyHandler(NextHistory,          "NextHistory") },
                { Keys.Minus,           MakeKeyHandler(PreviousHistory,      "PreviousHistory") },
                { Keys.Period,          MakeKeyHandler(RepeatLastCommand,    "RepeatLastCommand") },
                { Keys.Semicolon,       MakeKeyHandler(RepeatLastCharSearch, "RepeatLastCharSearch") },
                { Keys.Comma,           MakeKeyHandler(RepeatLastCharSearchBackwards, "RepeatLastCharSearchBackwards") },
                { Keys.AltH,            MakeKeyHandler(ShowParameterHelp,     "ShowParameterHelp") },
                { Keys.F1,              MakeKeyHandler(ShowCommandHelp,       "ShowCommandHelp") },
            };

            // Some bindings are not available on certain platforms
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _viCmdKeyMap.Add(Keys.CtrlDelete, MakeKeyHandler(KillWord, "KillWord"));
            }

            _viChordDTable = new Dictionary<PSKeyInfo, KeyHandler>
            {
                { Keys.D,               MakeKeyHandler( DeleteLine,                   "DeleteLine") },
                { Keys.Underbar,        MakeKeyHandler( DeleteLine,                   "DeleteLine") },
                { Keys.Dollar,          MakeKeyHandler( DeleteToEnd,                  "DeleteToEnd") },
                { Keys.B,               MakeKeyHandler( BackwardDeleteWord,           "BackwardDeleteWord") },
                { Keys.ucB,             MakeKeyHandler( ViBackwardDeleteGlob,         "ViBackwardDeleteGlob") },
                { Keys.W,               MakeKeyHandler( DeleteWord,                   "DeleteWord") },
                { Keys.ucW,             MakeKeyHandler( ViDeleteGlob,                 "ViDeleteGlob") },
                { Keys.E,               MakeKeyHandler( DeleteEndOfWord,              "DeleteEndOfWord") },
                { Keys.G,               MakeKeyHandler( ViDGChord,                    "ViDGChord") },
                { Keys.ucG,             MakeKeyHandler( DeleteEndOfBuffer,            "DeleteEndOfBuffer") },
                { Keys.ucE,             MakeKeyHandler( ViDeleteEndOfGlob,            "ViDeleteEndOfGlob") },
                { Keys.H,               MakeKeyHandler( BackwardDeleteChar,           "BackwardDeleteChar") },
                { Keys.J,               MakeKeyHandler( DeleteNextLines,              "DeleteNextLines") },
                { Keys.K,               MakeKeyHandler( DeletePreviousLines,          "DeletePreviousLines") },
                { Keys.L,               MakeKeyHandler( DeleteChar,                   "DeleteChar") },
                { Keys.Space,           MakeKeyHandler( DeleteChar,                   "DeleteChar") },
                { Keys._0,              MakeKeyHandler( BackwardDeleteLine,           "BackwardDeleteLine") },
                { Keys.Uphat,           MakeKeyHandler( DeleteLineToFirstChar,        "DeleteLineToFirstChar") },
                { Keys.Percent,         MakeKeyHandler( ViDeleteBrace,                "ViDeleteBrace") },
                { Keys.F,               MakeKeyHandler( ViDeleteToChar,               "ViDeleteToChar") },
                { Keys.ucF,             MakeKeyHandler( ViDeleteToCharBackward,       "ViDeleteToCharBackward") },
                { Keys.T,               MakeKeyHandler( ViDeleteToBeforeChar,         "ViDeleteToBeforeChar") },
                { Keys.ucT,             MakeKeyHandler( ViDeleteToBeforeCharBackward, "ViDeleteToBeforeCharBackward") },
            };

            _viChordCTable = new Dictionary<PSKeyInfo, KeyHandler>
            {
                { Keys.C,               MakeKeyHandler( ViReplaceLine,                    "ViReplaceLine") },
                { Keys.Dollar,          MakeKeyHandler( ViReplaceToEnd,                   "ViReplaceToEnd") },
                { Keys.B,               MakeKeyHandler( ViBackwardReplaceWord,            "ViBackwardReplaceWord") },
                { Keys.ucB,             MakeKeyHandler( ViBackwardReplaceGlob,            "ViBackwardReplaceGlob") },
                { Keys.W,               MakeKeyHandler( ViReplaceWord,                    "ViReplaceWord") },
                { Keys.ucW,             MakeKeyHandler( ViReplaceGlob,                    "ViReplaceGlob") },
                { Keys.E,               MakeKeyHandler( ViReplaceEndOfWord,               "ViReplaceEndOfWord") },
                { Keys.ucE,             MakeKeyHandler( ViReplaceEndOfGlob,               "ViReplaceEndOfGlob") },
                { Keys.H,               MakeKeyHandler( BackwardReplaceChar,              "BackwardReplaceChar") },
                { Keys.L,               MakeKeyHandler( ReplaceChar,                      "ReplaceChar") },
                { Keys.Space,           MakeKeyHandler( ReplaceChar,                      "ReplaceChar") },
                { Keys._0,              MakeKeyHandler( ViBackwardReplaceLine,            "ViBackwardReplaceLine") },
                { Keys.Uphat,           MakeKeyHandler( ViBackwardReplaceLineToFirstChar, "ViBackwardReplaceLineToFirstChar") },
                { Keys.Percent,         MakeKeyHandler( ViReplaceBrace,                   "ViReplaceBrace") },
                { Keys.F,               MakeKeyHandler( ViReplaceToChar,                  "ViReplaceToChar") },
                { Keys.ucF,             MakeKeyHandler( ViReplaceToCharBackward,          "ViReplaceToCharBackward") },
                { Keys.T,               MakeKeyHandler( ViReplaceToBeforeChar,            "ViReplaceToBeforeChar") },
                { Keys.ucT,             MakeKeyHandler( ViReplaceToBeforeCharBackward,    "ViReplaceToBeforeCharBackward") },
            };

            _viChordGTable = new Dictionary<PSKeyInfo, KeyHandler>
            {
                { Keys.G,               MakeKeyHandler( MoveToFirstLine,                    "MoveToFirstLine") },
            };

            _viChordYTable = new Dictionary<PSKeyInfo, KeyHandler>
            {
                { Keys.Y,               MakeKeyHandler( ViYankLine,            "ViYankLine") },
                { Keys.Dollar,          MakeKeyHandler( ViYankToEndOfLine,     "ViYankToEndOfLine") },
                { Keys.B,               MakeKeyHandler( ViYankPreviousWord,    "ViYankPreviousWord") },
                { Keys.ucB,             MakeKeyHandler( ViYankPreviousGlob,    "ViYankPreviousGlob") },
                { Keys.W,               MakeKeyHandler( ViYankNextWord,        "ViYankNextWord") },
                { Keys.ucW,             MakeKeyHandler( ViYankNextGlob,        "ViYankNextGlob") },
                { Keys.E,               MakeKeyHandler( ViYankEndOfWord,       "ViYankEndOfWord") },
                { Keys.ucE,             MakeKeyHandler( ViYankEndOfGlob,       "ViYankEndOfGlob") },
                { Keys.H,               MakeKeyHandler( ViYankLeft,            "ViYankLeft") },
                { Keys.L,               MakeKeyHandler( ViYankRight,           "ViYankRight") },
                { Keys.Space,           MakeKeyHandler( ViYankRight,           "ViYankRight") },
                { Keys._0,              MakeKeyHandler( ViYankBeginningOfLine, "ViYankBeginningOfLine") },
                { Keys.Uphat,           MakeKeyHandler( ViYankToFirstChar,     "ViYankToFirstChar") },
                { Keys.Percent,         MakeKeyHandler( ViYankPercent,         "ViYankPercent") },
            };

            _viChordDGTable = new Dictionary<PSKeyInfo, KeyHandler>
            {
                { Keys.G,               MakeKeyHandler( DeleteRelativeLines,   "DeleteRelativeLines") },
            };

            _viCmdChordTable = new Dictionary<PSKeyInfo, Dictionary<PSKeyInfo, KeyHandler>>();
            _viInsChordTable = new Dictionary<PSKeyInfo, Dictionary<PSKeyInfo, KeyHandler>>();

            _dispatchTable = _viInsKeyMap;
            _chordDispatchTable = _viInsChordTable;
            _viCmdChordTable[Keys.G] = _viChordGTable;
            _viCmdChordTable[Keys.D] = _viChordDTable;
            _viCmdChordTable[Keys.C] = _viChordCTable;
            _viCmdChordTable[Keys.Y] = _viChordYTable;

            _normalCursorSize = _console.CursorSize;
            if ((_normalCursorSize < 1) || (_normalCursorSize > 100))
            {
                // This is unlikely, but possible if the cursor size is set directly in
                // the registry.
                _normalCursorSize = 10;
            }
        }
    }
}
