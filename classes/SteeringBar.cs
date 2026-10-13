
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace ControllerMonitor;

public class SteeringBar : Control
{
    public static readonly StyledProperty<int> ValueProperty =
        AvaloniaProperty.Register<SteeringBar, int>(
            nameof(Value),
            defaultValue: 0);

    public static readonly StyledProperty<Color> BarColorProperty =
        AvaloniaProperty.Register<SteeringBar, Color>(
            nameof(BarColor),
            defaultValue: Colors.DodgerBlue);

    public static readonly StyledProperty<Color> BarBackgroundColorProperty =
        AvaloniaProperty.Register<SteeringBar, Color>(
            nameof(BarBackgroundColor),
            defaultValue: Color.FromRgb(50, 50, 50));

    private double displayedValue = 0;
    private DispatcherTimer? animationTimer;

    public int Value
    {
        get => this.GetValue(ValueProperty);
        set => this.SetValue(ValueProperty, Math.Clamp(value, -100, 100));
    }

    public Color BarColor
    {
        get => this.GetValue(BarColorProperty);
        set => this.SetValue(BarColorProperty, value);
    }

    public Color BarBackgroundColor
    {
        get => this.GetValue(BarBackgroundColorProperty);
        set => this.SetValue(BarBackgroundColorProperty, value);
    }

    public SteeringBar()
    {
        this.Height = 30;

        AffectsRender<SteeringBar>(
            ValueProperty,
            BarColorProperty,
            BarBackgroundColorProperty);

        this.animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };

        this.animationTimer.Tick += (_, _) =>
        {
            double difference = this.Value - this.displayedValue;

            if (Math.Abs(difference) < 0.5)
            {
                this.displayedValue = this.Value;
                this.animationTimer.Stop();
            }
            else
            {
                this.displayedValue += difference * 0.2;
            }

            this.InvalidateVisual();
        };
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ValueProperty)
        {
            this.animationTimer?.Start();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        double width = this.Bounds.Width;
        double height = this.Bounds.Height;
        double center = width / 2;

        // Background
        context.FillRectangle(
            new SolidColorBrush(this.BarBackgroundColor),
            new Rect(0, 0, width, height));

        // Filled section
        double fill = Math.Abs(this.displayedValue) / 100.0 * center;
        double x = this.displayedValue < 0 ? center - fill : center;

        context.FillRectangle(
            new SolidColorBrush(this.BarColor),
            new Rect(x, 0, fill, height));

        // Center line
        context.FillRectangle(
            Brushes.White,
            new Rect(center - 1, 0, 2, height));
    }
}
