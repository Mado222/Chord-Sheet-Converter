<h1>Chord Sheet Converter</h1>
<h2>Overview</h2>
<p>The <strong>Chord Sheet Converter</strong> is a flexible tool that converts chord sheet files from popular formats like <a target="_blank" rel="noopener noreferrer" href="https://opensong.org/"><strong>OpenSong</strong></a>, <a target="_blank" rel="noopener noreferrer" href="https://www.ultimate-guitar.com/"><strong>UltimateGuitar</strong></a>, and <a target="_blank" rel="noopener noreferrer" href="https://www.chordpro.org/"><strong>ChordPro</strong></a> into a richly formatted <strong>DOCX</strong> document. The tool allows you to generate highly customizable chord sheets, utilizing tags and styles to ensure a professional and polished output suitable for printing or sharing.</p>
<h2>Features</h2>
<ul>
    <li><strong>Multi-Format Support</strong>: Import chord sheet files from <a target="_blank" rel="noopener noreferrer" href="https://opensong.org/">OpenSong</a>, <a target="_blank" rel="noopener noreferrer" href="https://www.ultimate-guitar.com">UltimateGuitar</a>, and <a target="_blank" rel="noopener noreferrer" href="https://www.chordpro.org/">ChordPro</a>.</li>
    <li><strong>DOCX Export</strong>: Converts your chord sheets into a Microsoft Word document (DOCX) with professional formatting.</li>
    <li><strong>Tag-based Formatting</strong>: Use tags to apply various styles and formatting options, making the output highly customizable.</li>
    <li><strong>Flexible Styling</strong>: Define custom fonts, colors, and other style elements to ensure the generated chord sheet matches your needs.</li>
    <li><strong>Easy to Use</strong>: A simple and intuitive workflow for converting and customizing your chord sheets.</li>
</ul>
<h2>How It Works</h2>
<p>The Chord Sheet Converter reads chord sheet files in the following formats:</p>
<ul>
    <li><strong>OpenSong (.xml, . - no extension)</strong>: Import your OpenSong chord sheets.</li>
    <li><strong>UltimateGuitar (.txt)</strong>: Convert chord sheets from UltimateGuitar’s .crd format.</li>
    <li><strong>ChordPro (.cho)</strong>: Read and transform chord sheets written in the ChordPro format.</li>
</ul>
<p>Using <u>predefined tags</u> within your input files, the converter applies various styles to chords, lyrics, headers, and other elements and outputs a well-formatted DOCX file. The flexibility of the tagging system allows for fine-grained control over the final document's look and feel.</p>
<h2>Requirements</h2>
<p>The Chord Sheet Converter requires&nbsp;<span style="box-sizing:border-box;margin:0px;padding:0px;">installing either&nbsp;<strong>Microsoft Word</strong>&nbsp;or&nbsp;<strong>LibreOffice/OpenOffice</strong>&nbsp;to convert DOCX</span>&nbsp;to PDF format.</p>
<h2>Getting Started</h2>
<h3>Prerequisites</h3>
<p>To use the Chord Sheet Converter, you'll need:</p>
<ul>
    <li>.NET 8 SDK</li>
    <li>Microsoft Word, LibreOffice, or OpenOffice for DOCX to PDF conversion</li>
    <li>A text editor for customizing input files (optional)</li>
</ul>
<h3>Installation</h3>
<ol>
    <li>
        <p>Clone the repository:</p>
        <pre><code class="language-plaintext">git clone https://github.com/Mado222/chord-sheet-converter.git cd chord-sheet-converter </code></pre>
    </li>
    <li>Open the solution in <strong>Visual Studio</strong>.</li>
    <li>Build the project to restore dependencies and compile the application.</li>
</ol>
<h5><br>Alternatively:</h5>
<ul>
    <li>Download from the recent build setup.7z, unzip it and run setup.exe<br>&nbsp;</li>
</ul>
<h3>Usage</h3>
<ol>
    <li>Prepare your chord sheet files from OpenSong, UltimateGuitar, or ChordPro.</li>
    <li>Use the provided UI to load your chord sheet file.</li>
    <li>Customize / Optimize the tags and styles if needed.</li>
    <li>Generate the DOCX file.</li>
    <li>Open the DOCX in Microsoft Word to view, print, or further edit.</li>
</ol>
<h3>Example</h3>
<p>Open Song:</p>
<pre><code class="language-plaintext">&lt;?xml version="1.0" encoding="UTF-8"?&gt;
&lt;song&gt; 
	&lt;title&gt;Amazing Grace&lt;/title&gt; 
	&lt;author&gt;&lt;/author&gt; 
	&lt;copyright&gt;&lt;/copyright&gt;
	&lt;lyrics&gt;
[C]
.     G              C/G        G
 Amazing Grace, how sweet the sound,
.                         D7
 that saved a wretch like me.
.     G      G7        C       G
 I once was lost, but now am found,
.     Em        D7     G   C/D
 was blind, but now I see.
	&lt;/lyrics&gt;
  	&lt;time_sig&gt;&lt;/time_sig&gt;

&lt;/song&gt; 
 </code></pre>
<p>ChordPro:</p>
<pre><code class="language-plaintext">{ns}
{title: Amazing Grace}

{comment: Audience likes it}


{start_of_chorus: label="Chorus"}
Amazi[G]ng Grace, how s[C/G]weet the s[G]ound,
that saved a wretch like[D7] me.
I onc[G]e was l[G7]ost, but n[C]ow am f[G]ound,
was b[Em]lind, but [D7]now I s[G]ee.C/D
{eoc}</code></pre>
<p>UltimateGuitar:</p>
<pre><code class="language-plaintext">[Verse 1]
     F              Bb/F       F
Amazing Grace, how sweet the sound,
                         C7
that saved a wretch like me.
     F      F7        Bb      F
I once was lost, but now am found,
     Dm        C7     F   Bb/C
was blind, but now I see.

[Verse 2]
       F                    Bb       F
'Twas grace that taught my heart to fear,</code></pre>
<p>After processing, the output will be a neatly styled DOCX file, with chords and lyrics formatted according to the tags and style definitions.</p>
<h2>Configuration File</h2>
<p>The application stores user-specific settings in a JSON configuration file located in:</p>
<ul>
    <li><strong>AppData\Local\ChordSheetConverter\settings.json</strong>: Stores the user's default directories and other configuration parameters.</li>
</ul>
<h2>Default Directories</h2>
<p>By default, the following directories are created in the user's Documents folder:</p>
<ul>
    <li><strong>Templates Directory:</strong> <code>Documents/ChordSheetConverter/Templates</code> - stores DOCX template files for custom chord sheet formatting.</li>
    <li><strong>Output Directory:</strong> <code>Documents/ChordSheetConverter</code> - stores the generated DOCX files and other output files.</li>
</ul>
<h2>Customization</h2>
<p>Design your Template DOCX entirely to your needs. Hint: use <strong>Template1.docx&nbsp;</strong>as basis for all further custumization.</p>
<p>In the template<span style="box-sizing:border-box;margin:0px;padding:0px;">, use&nbsp;<strong>tags</strong>&nbsp;to specify where song information is inserted</span>. The following <strong>tags</strong> are supported:</p>
<ul>
    <li><code><strong>{songBody}</strong>: Section which will be filled with Lyrics and Chords.</code><br><br>&nbsp;</li>
    <li><code>{songTitle}</code>: Specifies the song title.</li>
    <li><code>{songAuthor}</code>: Specifies the artist/composer ….</li>
    <li><code>{songComment}</code>: Whatever you like.</li>
    <li><code>{songLyricist}</code></li>
    <li><code>{songCopyright}</code></li>
    <li><code>{songYear}</code></li>
    <li><code>{songKey}</code></li>
    <li><code>{songTime}</code></li>
    <li><code>{songTempo}</code></li>
    <li><code>{songCapo}</code></li>
</ul>
<p><span style="box-sizing:border-box;margin:0px;padding:0px;">A&nbsp;<strong>style</strong>&nbsp;can be included for every <strong>tag</strong>, which is automatically assigned to the <strong>tag</strong>.</span></p>
<p><code>Styles only related to&nbsp;<strong>{songBody}</strong></code></p>
<ul>
    <li><code>songText: Style for lyric lines - only used in <strong>{songBody}</strong></code></li>
    <li><code>songChords: Style for lines with chords&nbsp;- only used in&nbsp;<strong>{songBody}</strong></code></li>
    <li><code>songSectionBegin: Style for section labels like “Verse 1”, “Chorus”, - only used in&nbsp;<strong>{songBody}</strong></code></li>
    <li><code>songSectionEnd: Style for section labels like “End Verse 1”, “End Chorus”, - only used in <strong>{songBody}</strong></code></li>
</ul>
<p><code>Styles only related to <strong>tags:</strong></code></p>
<ul>
    <li>songTitle</li>
    <li>songAuthor</li>
    <li>songComment</li>
    <li>songLyricist</li>
    <li>songCopyright</li>
    <li>songYear</li>
    <li>songKey</li>
    <li>songTime</li>
    <li>songTempo</li>
    <li>songCapo</li>
</ul>
<p><strong><u>Important</u></strong>: All styles must be based on style "Standard" AND MUST BE DEFINED for <strong>paragraph&nbsp;and character.</strong></p>
<p>&nbsp;</p>
<h2>Contributing</h2>
<p>We welcome contributions to improve this tool! Open an issue if you have suggestions, feature requests, or bug reports.</p>
<h2>License</h2>
<p>This project is licensed under the <a target="_blank" rel="noopener noreferrer" href="https://opensource.org/license/MIT">MIT License</a>.</p>
