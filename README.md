<h1>Chord Sheet Converter</h1>
<h2>Overview</h2>
<p>The <strong>Chord Sheet Converter</strong> is a flexible tool designed to convert chord sheet files from popular formats like <strong>OpenSong</strong>, <strong>UltimateGuitar</strong>, and <strong>ChordPro</strong> into a richly formatted <strong>DOCX</strong> document. The tool allows you to generate highly customizable chord sheets, utilizing tags and styles to ensure a professional and polished output suitable for printing or sharing.</p>
<h2>Features</h2>
<ul>
    <li><strong>Multi-Format Support</strong>: Import chord sheet files from OpenSong, UltimateGuitar, and ChordPro.</li>
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
  &lt;aka&gt;&lt;/aka&gt;
  &lt;key_line&gt;&lt;/key_line&gt;
  &lt;linked_songs/&gt;
  &lt;time_sig&gt;&lt;/time_sig&gt;
  &lt;backgrounds resize="screen" keep_aspect="false" link="false" background_as_text="false"/&gt;
&lt;/song&gt;
</code></pre>
<p>After processing, the output will be a neatly styled DOCX file, with chords and lyrics formatted according to the tags and style definitions.</p>
<h2>Customization</h2>
<p>You can modify the default styles by adjusting the tags within the input files. The following tags are supported:</p>
<ul>
    <li><code>{title}</code>: Specifies the song title.</li>
    <li><code>{artist}</code>: Specifies the artist.</li>
    <li><code>{define}</code>: Defines chords.</li>
    <li><code>[chord]</code>: Wraps the chord to be displayed in the document.</li>
    <li><code>[section]</code>: Marks sections like verse, chorus, etc.</li>
</ul>
<p>For advanced styling, refer to the project’s documentation.</p>
<h2>Contributing</h2>
<p>We welcome contributions to improve this tool! If you have suggestions, feature requests, or bug reports, feel free to open an issue or submit a pull request.</p>
<h2>License</h2>
<p>This project is licensed under the MIT License. See the <code>LICENSE</code> file for more details.</p>
