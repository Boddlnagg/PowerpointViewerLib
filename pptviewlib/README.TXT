
PPTVIEWLIB - Control PowerPoint Viewer 2003/2007 (for openlp.org)
Copyright (C) 2008-2011 Jonathan Corwin (j@corwin.co.uk)

This library wrappers the free Microsoft PowerPoint Viewer (2003/2007) program,
allowing it to be more easily controlled from another program.

The PowerPoint Viewer must already be installed on the destination machine, and is
freely available at microsoft.com.

The full Microsoft Office PowerPoint and PowerPoint Viewer 97 have a COM interface allowing
automation. This ability was removed from the 2003+ viewer offerings.

To developers: I am not a C/C++ or Win32 API programmer as you can probably tell.
The code and API of this DLL could certainly do with some tidying up, and the
error trapping, where it exists, is very basic. I'll happily accept patches!

This library is covered by the GPL (http://www.gnu.org/licenses/)
It is NOT covered by the LGPL, so can only be used in GPL compatable programs.
(http://www.gnu.org/licenses/why-not-lgpl.html)

This README.TXT must be distributed with the pptviewlib.dll

This library has a limit of 50 PowerPoints which can be opened simultaneously.

This project can be built with the free Microsoft Visual C++ 2008 Express Edition.

USAGE
-----
BOOL CheckInstalled(void);
    Returns TRUE if PowerPointViewer is installed. FALSE if not.

int OpenPPT(char *filename, HWND hParentWnd, RECT rect, char *previewpath);

    Opens the PowerPoint file, counts the number of slides, sizes and positions accordingly
    and creates preview images of each slide. Note PowerPoint Viewer only allows the
    slideshow to be resized whilst it is being loaded. It can be moved at any time however.

    The only way to count the number of slides is to step through the entire show. Therefore
    there will be a delay whilst opening large presentations for the first time.
    For pre XP/2003 systems, the slideshow will flicker as the screen snapshots are taken.

    filename:    The PowerPoint file to be opened. Full path
    hParentWnd:  The window which will become the parent of the slideshow window.
                 Can be NULL.
    rect:        The location/dimensions of the slideshow window.
                 If all properties of this structure are zero, the dimensions of the hParentWnd
                 are used.
    previewpath  If specified, the prefix to use for snapshot images of each slide, in the
                 form: previewpath + n + ".bmp", where n is the slide number.
                 A file called previewpath + "info.txt" will also be created containing information
                 about the PPT file, to speed up future openings of the unmodified file.
                 Note it is up the calling program to directly access these images if they
                 are required.

    RETURNS:     An unique identifier to pass to other methods in this library.
                 If < 0, then the PPT failed to open.
                 If >=0, ClosePPT must be called when the PPT is no longer being used
                 or when the calling program is closed to release resources/hooks.

void ClosePPT(int id);
     Closes the presentation, releasing any resources and hooks.

     id: The value returned from OpenPPT.

int GetCurrentSlide(int id);
    Returns the current slide number (from 1)

    id: The value returned from OpenPPT.

int GetSlideCount(int id);
    Returns the total number of slides.

    id: The value returned from OpenPPT.

void NextStep(int id);
    Advances one step (animation) through the slideshow.

    id: The value returned from OpenPPT.

void PrevStep(int id);
    Goes backwards one step (animation) through the slideshow.

    id: The value returned from OpenPPT.

void GotoSlide(int id, int slideno);
    Goes directly to a specific slide in the slideshow

    id: The value returned from OpenPPT.
    slideno: The number of the slide (from 1) to go directly to.

    If the slide has already been displayed, then the completed slide with animations performed
    will be shown. This is how the PowerPoint Viewer works so have no control over this.

void RestartShow(int id);
    Restarts the show from the beginning. To reset animations, behind the scenes it
    has to travel to the end and step backwards though the entire show. Therefore
    for large presentations there might be a delay.

    id: The value returned from OpenPPT.

void Blank(int id);
    Blanks the screen, colour black.

    id: The value returned from OpenPPT.

void Unblank(int id)
    Unblanks the screen, restoring it to it's pre-blank state.

    id: The value returned from OpenPPT.

void Stop(int id)
    Moves the slideshow off the screen. (There is no concept of stop show in the PowerPoint Viewer)

    id: The value returned from OpenPPT.

void Resume(int id)
    Moves the slideshow display back onto the screen following a Stop()

    id: The value returned from OpenPPT.

