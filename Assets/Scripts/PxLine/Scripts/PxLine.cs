public struct PxLine {
    public bool ready;
    public int sx;
    public int sy;
    public int ex;
    public int ey;

    public int width => ex - sx;
    public int height => ey - sy;
    public int size => width > height ? width : height;
}
