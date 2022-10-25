using System.Text;

class Scanner : System.IO.StringReader {
    private string currentWord;

    public Scanner(string source) : base(source) {
        ReadNextWord();
    }

    private void ReadNextWord() {
        StringBuilder sb = new StringBuilder();
        char nextChar;
        int next;
        do {
            next = this.Read();
            if (next < 0)
                break;
            nextChar = (char)next;
            if (char.IsWhiteSpace(nextChar))
                break;
            sb.Append(nextChar);
        } while (true);

        while (this.Peek() >= 0 && char.IsWhiteSpace((char) this.Peek())) {
            this.Read();
        }

        if (sb.Length > 0) {
            currentWord = sb.ToString();
        } else {
            currentWord = null;
        }
    }

    public int NextInt() {
        try {
            return int.Parse(currentWord);
        } finally {
            ReadNextWord();
        }
    }

    public double NextDouble() {
        try {
            return double.Parse(currentWord);
        } finally {
            ReadNextWord();
        }
    }

    public bool HasNext() {
        return currentWord != null;
    }
}