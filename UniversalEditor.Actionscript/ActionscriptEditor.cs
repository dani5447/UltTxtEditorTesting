using System;
using System.Windows.Media;
using TextEditor;
using TextEditor.ScriptHighlight;
using UniversalEditor.Base;
using UniversalEditor.Base.FileSystem;

namespace UniversalEditor.Actionscript
{
	public class ActionscriptEditor : TextEditorBase
	{
		public ActionscriptEditor(IEditorOwner owner, FileWrapper file)
			: base(owner, file)
		{ }

		protected override UfeTextBox CreateTextBox()
		{
			return new UfeTextBox(_owner, CreateDocument(_file));
		}

		private static ScriptHighlightDocument CreateDocument(FileWrapper file)
		{
			ScriptHighlightInfo highlightInfo = new ScriptHighlightInfo();
			highlightInfo.CommentLine = "//";
			highlightInfo.Add(Brushes.Blue, "add for lt tellTarget and function ne this break ge new typeof continue gt not var delete if on void do ifFrameLoaded onClipEvent while else in or with eq le return instanceof case default switch");
			highlightInfo.Add(Brushes.Violet, "arguments constructor class dynamic false extends implements import interface intrinsic newline null private public super static true undefined Accessibility Arguments Array Boolean Button Camera ContextMenu ContextMenuItem CustomActions Color Date Error Function Key LoadVars LocalConnection Math Microphone Mouse MovieClip MovieClipLoader NetConnection NetStream Number PrintJob Object TextField StyleSheet TextFormat TextSnapshot SharedObject Selection Sound Stage String System XML XMLNode XMLSocket Void abs acos asin atan atan2 ceil cos floor log max min pow random round sin sqrt tan onActivity onChanged onClose onConnect onData onDragOut onDragOver onEnterFrame onID3 onKeyDown onKeyUp onKillFocus onLoad onLoadComplete onLoadError onLoadInit onLoadProgress onLoadStart onMouseDown onMouseMove onMouseUp onMouseWheel onPress onRelease onReleaseOutside onResize onRollOut onRollOver onScroller onSelect onSetFocus onSoundComplete onStatus onUnload onUpdate onXML addListener addPage addProperty addRequestHeader allowDomain allowInsecureDomain appendChild apply applyChanges asfunction attachAudio attachMovie attachSound attachVideo beginFill beginGradientFill call charAt charCodeAt clear clearInterval cloneNode close concat connect copy createElement createEmptyMovieClip createTextField createTextNode curveTo domain duplicateMovieClip endFill escape eval evaluate exp findText fscommand flush fromCharCode get getAscii getBeginIndex getBounds getBytesLoaded getBytesTotal getCaretIndex getCode getCount getDate getDay getDepth getEndIndex getFocus getFontList getFullYear getHours getInstanceAtDepth getLocal getMilliseconds getMinutes getMonth getNewTextFormat getNextHighestDepth getPan getProggress getProperty getRGB getSeconds getSelected getSelectedText getSize getStyle getStyleNames getSWFVersion getText getTextExtent getTextFormat getTextSnapshot getTime getTimer getTimezoneOffset getTransform getURL getUTCDate getUTCDay getUTCFullYear getUTCHours getUTCMilliseconds getUTCMinutes getUTCMonth getUTCSeconds getVersion getVolume getYear globalToLocal gotoAndPlay gotoAndStop hasChildNodes hide hideBuiltInItems hitTest hitTestTextNearPos indexOf insertBefore install isActive isDown isToggled join lastIndexOf lineStyle lineTo list load loadClip loadMovie loadMovieNum loadSound loadVariables loadVariablesNum localToGlobal mbchr mblength mbord mbsubstring MMExecute moveTo nextFrame nextScene parseCSS parseFloat parseInt parseXML pause play pop pow prevScene print printAsBitmap printAsBitmapNum printNum push random registerClass removeListener removeMovieClip removeNode removeTextField replaceSel replaceText reverse round seek send sendAndLoad setBufferTime set setDate setFocus setFullYear setGain setHours setInterval setMask setMilliseconds setMinutes setMode setMonth setMotionLevel setNewTextFormat setPan setProperty setQuality setRate setRGB setSeconds setSelectColor setSelected setSelection setSilenceLevel setStyle setTextFormat setTime setTransform setUseEchoSuppression setUTCDate setUTCFullYear setUTCHours setUTCMilliseconds setUTCMinutes setUTCMonth setUTCSeconds setVolume setYear shift show showSettings silenceLevel silenceTimeout sin slice sort sortOn splice split sqrt start startDrag stop stopAllSounds stopDrag substr substring swapDepths tan toggleHighQuality toLowerCase toString toUpperCase trace unescape uninstall unLoadClip unloadMovie unloadMovieNum unshift unwatch updateAfterEvent updateProperties useEchoSuppression valueOf watch endinitclip include initclip __proto__ _accProps _alpha _currentframe _droptarget _focusrect _framesloaded _global _height _highquality _level _lockroot _name _parent _quality _root _rotation _soundbuftime _target _totalframes _url _visible _width _x _xmouse _xscale _y _ymouse _yscale activityLevel align allowDomain allowInsecureDomain attributes autoSize avHardwareDisable background backgroundColor bandwidth blockIndent bold border borderColor bottomScroll bufferLenght bufferTime builtInItems bullet bytesLoaded bytesTotal callee caller capabilities caption childNodes color condenseWhite contentType currentFps customItems data deblocking docTypeDecl duration embedFonts enabled exactSettings firstChild focusEnabled font fps gain globalStyleFormat hasAccessibility hasAudio hasAudioEncoder hasEmbeddedVideo hasMP3 hasPrinting hasScreenBroadcast hasScreenPlayback hasStreamingAudio hasStreamingVideo hasVideoEncoder height hitArea hscroll html htmlText indent index italic instanceof int ignoreWhite isDebugger isDown isFinite italic language lastChild leading leftMargin length loaded localFileReadDisable manufacturer maxChars maxhscroll maxscroll menu message motionLevel motionTimeout mouseWheelEnabled multiline muted name names NaN nextSibling nodeName nodeType nodeValue os parentNode password pixelAspectRatio playerType previousSibling prototype quality rate restrict resolutionX resolutionY rightMargin scaleMode screenColor screenDPI screenResolutionX screenResolutionY scroll selectable separatorBefore showMenu size smoothing status styleSheet tabChildren tabEnabled tabIndex tabStops target targetPath text textColor textHeight textWidth time trackAsMenu type underline url useCodepage useEchoSuppression useHandCursor variable version visible width wordWrap xmlDecl");
		
			ScriptHighlightDocument result = new ScriptHighlightDocument(file, highlightInfo);
			return result;
		}

		public override void Focus()
		{
			_textBox.Value.TextBox.UpdateLayout();
			_textBox.Value.TextBox.Focus();
		}
	}
}
