# ElitePOS-Enterprise Design System

```yaml
name: ElitePOS-Enterprise
version: "1.0.0"
description: "High-frequency transaction point-of-sale system for desktop countertop terminals with tactile touch controls."
colors:
  primary: "#1A73E8"
  primary-hover: "#1557B0"
  background: "#FAF7F2"
  surface: "#FFFFFF"
  border: "#E8E3DA"
  success: "#266427"
  warning: "#E65100"
  error: "#EA4335"
  text-primary: "#1F1F1B"
  text-secondary: "#6B6B65"
  accent-sage: "#8AA890"
  surface-dark: "#1E1E1E"
  background-dark: "#121212"
  text-primary-dark: "#F5F5F5"
  text-secondary-dark: "#A0A09A"
typography:
  fontFamily: "Inter, system-ui, -apple-system, sans-serif"
  h1:
    fontSize: "28px"
    fontWeight: "600"
    lineHeight: "36px"
  h2:
    fontSize: "20px"
    fontWeight: "600"
    lineHeight: "28px"
  body-md:
    fontSize: "16px"
    fontWeight: "400"
    lineHeight: "24px"
  body-sm:
    fontSize: "14px"
    fontWeight: "400"
    lineHeight: "20px"
  caption:
    fontSize: "13px"
    fontWeight: "500"
    lineHeight: "18px"
    letterSpacing: "0.4px"
rounded:
  sm: "4px"
  md: "8px"
  lg: "12px"
  xl: "16px"
spacing:
  xs: "4px"
  sm: "8px"
  md: "12px"
  lg: "16px"
  xl: "24px"
  xxl: "32px"
components:
  touch-target:
    minWidth: "48px"
    minHeight: "48px"
    padding: "{spacing.lg}"
  product-card:
    backgroundColor: "{colors.surface}"
    border: "1px solid {colors.border}"
    rounded: "{rounded.xl}"
    padding: "{spacing.lg}"
  action-button:
    backgroundColor: "{colors.primary}"
    textColor: "#FFFFFF"
    rounded: "{rounded.md}"
    height: "56px"
```

## ElitePOS — Design System Guidelines

This system enforces strict WCAG 2.2 AA accessibility and physical touch ergonomics.

### Interaction Principles
- All interactive targets must be at least 48x48px to prevent touch registration errors.
- Text must never drop below 13px in caption areas.
- Primary CTA buttons must always have a high contrast ratio exceeding 4.5:1 against surfaces.
- Avoid fancy animations; transitions should be snappier than 150ms.
