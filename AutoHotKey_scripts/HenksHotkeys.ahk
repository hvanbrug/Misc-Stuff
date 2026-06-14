#Requires AutoHotkey v2.0
#SingleInstance Force

;@Ahk2Exe-SetMainIcon Images\HenksHotkeys.ico
;@Ahk2Exe-SetVersion  "2.3"
;@Ahk2Exe-SetName     "Henk's Hotkeys"

A_MaxHotkeysPerInterval := 400

#Include "SupportFiles\UIConstants.ahk"
#Include "SupportFiles\Utilities.ahk"
#Include "SupportFiles\Theme.ahk"
#Include "SupportFiles\Symbols.ahk"
#Include "SupportFiles\Emojis.ahk"
#Include "SupportFiles\CommentSupport.ahk"
#Include "SupportFiles\PromptHelpers.ahk"
#Include "SupportFiles\Greek.ahk"
#Include "SupportFiles\Russian.ahk"
#Include "SupportFiles\Misc.ahk"
#Include "SupportFiles\Tools.ahk"
#Include "SupportFiles\Sensitive.ahk"
#Include "SupportFiles\EmojiSupport.ahk"
#Include "EmojiResources.ahk"
#Include "SupportFiles\UITabPage.ahk"
#Include "SupportFiles\UI.ahk"
#Include "SupportFiles\UIScrolling.ahk"
#Include "SupportFiles\IniFile.ahk"
#Include "SupportFiles\Startup.ahk"
; usage = 72% - 57% = 15%
; credits spent = 249.8 + 155.6 + 364.4 = 769.8
; total credits = (769.8 / 15)  = 5132
;  100 credits  = ( 100 / 5132) =  1.95% of usage
;  200 credits  = ( 200 / 5132) =  3.89% of usage
;  300 credits  = ( 300 / 5132) =  5.84% of usage
;  500 credits  = ( 500 / 5132) =  9.74% of usage
; 1000 credits  = (1000 / 5132) = 19.48% of usage
; 2000 credits  = (2000 / 5132) = 38.96% of usage
; 3000 credits  = (3000 / 5132) = 58.44% of usage
; 5000 credits  = (5000 / 5132) = 97.39% of usage
;
;
;