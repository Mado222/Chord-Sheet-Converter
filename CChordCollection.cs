using System.Collections;
using System.Text;

namespace ChordSheetConverter
{
    public class CChord(string chord, int position)
    {
        public string Chord { get; set; } = chord;
        public int Position { get; set; } = position;
        public int Length { get => Chord.Length; }

        // Optional: override ToString() for easy display of CChord details
        public override string ToString()
        {
            return $"Chord: {Chord}, Position: {Position}, Length: {Length}";
        }
    }

    public class CChordCollection : IEnumerable<CChord>
    {
        private readonly List<CChord> allChords;

        public CChordCollection()
        {
            allChords = [];
        }

        // Add a chord to the collection
        public void AddChord(CChord chord)
        {
            allChords.Add(chord);
        }

        // Method to retrieve a well-spaced chord line
        public string GetWellSpacedChordLine()
        {
            var chordLine = new StringBuilder();
            int currentPosition = 0;

            foreach (var chord in allChords)
            {
                // Add spaces to reach the chord's starting position
                int spacesToAdd = chord.Position - currentPosition;
                if (spacesToAdd > 0)
                {
                    chordLine.Append(' ', spacesToAdd);
                }

                // Append the chord and update the current position
                chordLine.Append(chord.Chord);
                currentPosition = chord.Position + chord.Chord.Length;
            }

            return chordLine.ToString();
        }

        // IEnumerable implementation
        public IEnumerator<CChord> GetEnumerator()
        {
            return allChords.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}