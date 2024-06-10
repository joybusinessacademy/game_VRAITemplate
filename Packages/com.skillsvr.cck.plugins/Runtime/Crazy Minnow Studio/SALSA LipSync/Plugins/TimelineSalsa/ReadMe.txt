Timeline SALSA Core Add-on: Requires Unity v2017.1+

RELEASE NOTES & TODO ITEMS:
		2.6.2:
			~ Cosmetic changes to Inspector
            ~ Wording changes to Inspector fields and tips for clarity.
		2.6.1:
			! ExposedReference<Transform> lookTarget implementation to ensure references persist through scene and Unity loading.
            ~ "filter" verbiage changed to "excluded" to clarify intent of function.
            ~ Inspector cosmetics and restructuring to accomodate ExposedReference drawing by Unity.
		2.6.0:
			+ EyesControl: filter option on applicable settings to prevent application of clip settings changes when not desirable.
		2.5.1:
			~ Remove log warning from drawers on binding missing. Unity v2020.LTS+ parses the drawer before behavior
				initialization throwing the warning for no good reason.
			~ Refactored some internal naming for clarity.
			! EmoterControl behaviors now properly display clip drawer in Unity v2020.LTS+.
		2.5.0:
			+ Added EyesControl.
			+ Added SalsaControl (replaces SalsaAudio [now deprecated]).
			+ Added EmoterControl (replaces Emote [now deprecated]).
			~ Emote and SalsaAudio track/mixer/clip/drawer no longer included in the Timeline add-on distribution.
		2.2.0:
			+ Added PlayableDirector.Pause()/Play() functionality back to SalsaAudio clip types.
		2.1.0:
			~ Inspector changes to support Unity 2019.3+
			~ Emote Clip changes now indicate leading/trailing edges for one-way emotes.
			~ Updated scene. No longer requires Examples pack.
			+ Help text to better explain Emote Clip configuration (one-way/two-way) requirements.
        2.0.2-beta:
            ~ scene changes to work with new GUIDs from Examples pack.
        2.0.1-beta:
            ~ scene changes to work with guids from core packages.
        2.0.0-beta:
            + initial release for SALSA LipSync Suite.

LOCATION OF FILES:
	Assets\Crazy Minnow Studio\Addons\TimelineSalsa
	Assets\Crazy Minnow Studio\Examples\Scenes      (if applicable)
	Assets\Crazy Minnow Studio\Examples\Scripts     (if applicable)

========================================================================================
INSTRUCTIONS:
	(visit https://crazyminnowstudio.com/docs/salsa-lip-sync/ for the latest info)
    To extend/modify these files, copy their contents to a new set of files and
    use a different namespace to ensure there are no scoping conflicts if/when this
    add-on is updated.
SUPPORT: Contact assetsupport@crazyminnow.com. Provide:
    1) your purchase email and invoice number
    2) version numbers (OS, Unity, SALSA, etc.)
    3) full details surrounding the problem you are experiencing.
    4) relevant information for what you have tried/implemented.
    Support is only provided for Crazy Minnow Studio products with valid
	proof of purchase.

PURPOSE: This script provides a very simple Unity Timeline implementation
	for SALSA AudioSources and emote group firing.
KNOWN ISSUES: none.

========================================================================================
DISCLAIMER: While every attempt has been made to ensure the safe content
	and operation of these files, they are provided as-is, without
	warranty or guarantee of any kind. By downloading and using these
	files you are accepting any and all risks associated and release
	Crazy Minnow Studio, LLC of any and all liability.
========================================================================================
