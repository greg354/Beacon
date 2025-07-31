using System;
using System.Drawing;
using System.Windows.Forms;

namespace Beacon.Controls;

public class WarningTooltip : ToolTip {
  private const TextFormatFlags toolTipFlags = TextFormatFlags.Left;
  private static readonly Font toolTipFont = new("Jetbrains Mono", 10F, FontStyle.Regular);

  public WarningTooltip() {
    this.OwnerDraw = true;
    Popup += onPopup;
    Draw += onDraw;
  }

  private static void onPopup(object sender, PopupEventArgs eventArgs) {
    // setting size relative to tooltip text https://stackoverflow.com/a/53745899
    var toolTipText = (sender as ToolTip)?.GetToolTip(eventArgs.AssociatedControl);
    using var graphics = eventArgs.AssociatedControl.CreateGraphics();
    // in our draw, we split the tooltip at every new line and manually position them
    // to display an accompanied icon, we have to account for this in our size estimates
    var toolTipLines = toolTipText!.Split('\n');
    var maxWidth = 0;
    var totalHeight = 0;
    const int lineSpacing = 2;
    foreach (var line in toolTipLines) {
      var lineSize = TextRenderer.MeasureText(line, toolTipFont, Size.Empty, toolTipFlags);
      var lineWidthWithIcon = lineSize.Width + 20;
      maxWidth = Math.Max(maxWidth, lineWidthWithIcon);
      totalHeight += lineSize.Height + lineSpacing;
    }

    totalHeight -= lineSpacing;
    var paddedSize = new Size(maxWidth + 30, totalHeight + 40);
    eventArgs.ToolTipSize = paddedSize;
  }

  private static void onDraw(object sender, DrawToolTipEventArgs eventArgs) {
    var graphics = eventArgs.Graphics;
    using (var solidBrush =
           new SolidBrush(MainForm.backColour)) {
      graphics.FillRectangle(solidBrush, eventArgs.Bounds);
    }

    ControlPaint.DrawBorder(graphics, eventArgs.Bounds, Color.RoyalBlue, ButtonBorderStyle.Solid);
    TextRenderer.DrawText(graphics, @"Warnings", new Font(toolTipFont, FontStyle.Bold),
      new Point(eventArgs.Bounds.X + 10, eventArgs.Bounds.Y + 10), Color.White,
      TextFormatFlags.Left);

    var splitStrings = eventArgs.ToolTipText.Split('\n');
    var yPosition = eventArgs.Bounds.Top + 20;
    foreach (var line in splitStrings) {
      // var tooltipText = line.Replace("\n", @"");
      var linePosition = new Point(eventArgs.Bounds.Left + 10, yPosition + 10);
      TextRenderer.DrawText(graphics, @"⚠", new Font("Jetbrains Mono", 13f, FontStyle.Bold),
        new Point(linePosition.X, linePosition.Y - 3), Color.Orange,
        TextFormatFlags.Left);
      var textBounds =
        new Rectangle(new Point(linePosition.X + 20, linePosition.Y), eventArgs.Bounds.Size);
      TextRenderer.DrawText(graphics, line, toolTipFont, textBounds, Color.DarkGray, toolTipFlags);
      yPosition += 20;
    }
  }
}