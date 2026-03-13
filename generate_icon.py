from PIL import Image, ImageDraw, ImageFont
import os

def create_icon(filename, bg_color, text_color, text):
    # Sizes needed for a proper Windows .ico file
    sizes = [(16, 16), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)]
    images = []

    for size in sizes:
        img = Image.new('RGBA', size, (255, 255, 255, 0)) # transparent background
        draw = ImageDraw.Draw(img)

        # Draw a rounded rectangle or circle for the logo base
        margin = max(1, int(size[0] * 0.1))
        # Circle:
        draw.ellipse([margin, margin, size[0]-margin, size[1]-margin], fill=bg_color)

        # We don't have a default font that is guaranteed to work cleanly across all OS
        # without specifying path, so we'll use default bitmap if truetype fails
        try:
            # Try a default font (usually available in many linux distros if installed)
            font_size = int(size[0] * 0.5)
            font = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf", font_size)
        except IOError:
            font = ImageFont.load_default()

        # Center the text
        left, top, right, bottom = draw.textbbox((0, 0), text, font=font)
        w = right - left
        h = bottom - top
        x = (size[0] - w) / 2
        y = (size[1] - h) / 2
        # Adjust Y slightly up due to font baselines
        if size[0] > 32:
            y -= size[1] * 0.05

        draw.text((x, y), text, fill=text_color, font=font)
        images.append(img)

    # Save as .ico
    images[0].save(filename, format='ICO', sizes=sizes, append_images=images[1:])

# Procto Main (Blue theme)
create_icon('SafeExamCEF/app.ico', bg_color=(25, 118, 210, 255), text_color=(255, 255, 255, 255), text="P")

# Procto Lite (Green theme)
create_icon('ProctoLite/app.ico', bg_color=(56, 142, 60, 255), text_color=(255, 255, 255, 255), text="PL")

print("Icons generated successfully!")
