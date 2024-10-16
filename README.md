======= Work in progress Software works basically lots of improvements oopen ;-) ====================
<h1>Chord Sheet Converter</h1>
<h2>Overview</h2>
<p>The <strong>Chord Sheet Converter</strong> is a flexible tool designed to convert chord sheet files from popular formats like <a target="_blank" rel="noopener noreferrer" href="https://opensong.org/"><strong>OpenSong</strong></a>, <a target="_blank" rel="noopener noreferrer" href="https://www.ultimate-guitar.com/"><strong>UltimateGuitar</strong></a>, and <a target="_blank" rel="noopener noreferrer" href="https://www.chordpro.org/"><strong>ChordPro</strong> </a>into a richly formatted <strong>DOCX</strong> document. The tool allows you to generate highly customizable chord sheets, utilizing tags and styles to ensure a professional and polished output suitable for printing or sharing.</p>
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
    <li><strong>OpenSong (. - no extension)</strong>: Import your OpenSong chord sheets.</li>
    <li><strong>UltimateGuitar (.txt)</strong>: Convert chord sheets from UltimateGuitar’s .crd format.</li>
    <li><strong>ChordPro (.cho)</strong>: Read and transform chord sheets written in the ChordPro format.</li>
</ul>
<p>Using predefined tags within your input files, the converter applies various styles to chords, lyrics, headers, and other elements, and outputs a well-formatted DOCX file. The flexibility of the tagging system allows for fine-grained control over the look and feel of the final document.</p>
<h2>Getting Started</h2>
<h3>Prerequisites</h3>
<p>To use the Chord Sheet Converter, you'll need:</p>
<ul>
    <li>.NET 8 SDK</li>
    <li>A text editor for customizing input files (optional)</li>
    <li>Microsoft Word (or any compatible DOCX editor) to view and print the output</li>
</ul>
<h3>Installation</h3>
<ol>
    <li>
        <p>Clone the repository:</p>
        <pre><code class="language-plaintext">git clone https://github.com/yourusername/chord-sheet-converter.git
cd chord-sheet-converter
</code></pre>
    </li>
    <li>Open the solution in <strong>Visual Studio</strong>.</li>
    <li>Build the project to restore dependencies and compile the application.</li>
</ol>
<h3>Usage</h3>
<ol>
    <li>Prepare your chord sheet files from OpenSong, UltimateGuitar, or ChordPro.</li>
    <li>Use the provided UI or command-line tool to load your chord sheet file.</li>
    <li>Customize the tags and styles if needed.</li>
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
 
 ;Comment
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
  &lt;key&gt;&lt;/key&gt;
  &lt;key_line&gt;&lt;/key_line&gt;
&lt;/song&gt;
</code></pre>
<p>ChordPro:</p>
<pre><code class="language-plaintext">{title: Paradise}
When [C]I was a child my [F]family would [C]travel,
down to Western Kentucky where my [G7]parents were [C]born
And there's a backwards old town that's [F]often re[C]membered,
so many times that my [G7]memories are [C]worn.</code></pre>
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
'Twas grace that taught my heart to fear,
                      C7
and grace my fears relieved.
       F     F7        Bb      F
How precious did that grace appear, 
    Dm      C7     F      D7
the hour I first believed.
</code></pre>
<p>After processing, the output will be a neatly styled DOCX file, with chords and lyrics formatted according to the tags and style definitions.</p>
<h2>Customization</h2>
<p>Design your Template DOCX completely to your needs - <strong>see Template1.docx</strong>. (One column, 2 columns …. etc)</p>
<p>Use tags to specify the place where song information comes in.</p>
<ul>
    <li><code>{SongTitle}</code>: Specifies the song title.</li>
    <li><code>{SongAuthor}</code>: Specifies the artist/composer/….</li>
    <li><code>{SongBody}</code>: Section with Lytics and Chords</li>
    <li>
        <p><code>{SongComment}</code>: Whatever you like</p>
        <p>&nbsp;</p>
    </li>
</ul>
<p>Create styles with the following names:</p>
<ul>
    <li><code>SongTitle</code>: Style for {SongTitle}</li>
    <li><code>SongAuthor</code>: Style for {SongAuthor}</li>
    <li><code>SongComment</code>: Style for {<code>SongComment</code>}</li>
    <li><code>SongChords</code>: Style for lines with chords</li>
    <li><code>SongText</code>: Style for lyric lines</li>
    <li><code>SongSection</code>: Style for section labels like “Vers 1”, “Chorus”, …</li>
</ul>
<p>&nbsp;</p>
<h2>Contributing</h2>
<p>We welcome contributions to improve this tool! If you have suggestions, feature requests, or bug reports, feel free to open an issue or submit a pull request.</p>
<h2>License</h2>
<p>This project is licensed under the MIT License. See the <code>LICENSE</code> file for more details.</p>
