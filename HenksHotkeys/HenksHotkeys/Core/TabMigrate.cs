namespace HenksHotkeys.Core;

/// <summary>
/// One-time, in-memory upgrade of a pre-coordinate tabs.json (rows of buttons, with
/// blank spacer cells/rows and fractional gaps) to the coordinate format: a flat list
/// of buttons each carrying an explicit (row, col), plus section dividers at row
/// indices. Blank cells, empty/blank rows and fractional gaps are dropped — vertical
/// gaps survive as skipped row indices, horizontal position as the column index. After
/// migration the legacy <see cref="TabEntry.Rows"/> is cleared so it's never written
/// again. Idempotent: a tab already in the new format (Rows == null) is left alone.
/// </summary>
internal static class TabMigrate
{
  /// <summary>Migrate every legacy (rows-based) data tab in <paramref name="file"/> to
  /// the coordinate format. Returns true if anything was converted (so the caller can
  /// rewrite the file once).</summary>
  public static bool Migrate( TabFile file )
  {
    bool changed = false;
    foreach( TabEntry t in file.Tabs )
    {
      if( t.Rows is not null )
      {
        MigrateTab( t );
        changed = true;
      }
    }
    return changed;
  }

  private static void MigrateTab( TabEntry t )
  {
    // Heal any over-wide legacy rows first (so the produced columns stay in bounds),
    // then walk the rows top-to-bottom assigning integer grid coordinates.
    VersionStamp.NormalizeRows( t );

    var buttons  = new List<ButtonDef>();
    var sections = new List<SectionDef>();
    int r = 0;

    foreach( RowDef row in t.Rows! )
    {
      // A row's gapBefore was extra vertical space ahead of it (in row-height units):
      // round it to whole skipped row indices. Fractional group-gaps round away to 0.
      r += Math.Max( 0, (int)Math.Round( row.GapBefore ) );

      if( row.IsSection )
      {
        sections.Add( new SectionDef
        {
          Row    = r,
          Name   = row.Section ?? "",
          Height = row.HeaderHeight,
        } );
        r++;
        continue;
      }

      if( row.Blank )
      {
        r++;          // a blank spacer row → one skipped (empty) row index
        continue;
      }

      int col = row.Indent;
      foreach( ButtonDef b in row.Buttons )
      {
        col += Math.Max( 0, (int)Math.Round( b.GapBefore ) );  // horizontal gap → skipped cols
        if( b.Blank )
        {
          col++;      // a blank spacer cell → one skipped (empty) column
          continue;
        }

        b.Row      = r;
        b.Col      = col;
        b.GapBefore = 0;       // legacy fields cleared (never written in the new format)
        b.Blank     = false;
        buttons.Add( b );

        col += Math.Max( 1, b.Width );   // a wide button occupies its full width
      }
      r++;
    }

    t.Buttons  = buttons;
    t.Sections = sections.Count > 0 ? sections : null;
    t.Rows     = null;          // legacy layout consumed
  }
}
