# UX Polish Pass

## UI Stack
- Framework: ASP.NET Core MVC (Razor views)
- CSS: Bootstrap + custom stylesheet (`wwwroot/css/site.css`)
- JS: vanilla (`wwwroot/js/site.js`), Bootstrap bundle

## Shared Components
- Buttons: `.button`, `.btn`, `.button.ghost`, `.button.danger`, `.text-link`
- Inputs: `.form-control`, `.form-select`, `textarea.form-control`
- Cards/Panels: `.panel`, `.card`, `.panel-header`
- Tables: `.table`, `.table-wrap`
- Layout: `.page-header`, `.content-grid`, `.form-grid`, `.details-grid`, `.timeline`

## UX Audit
### Layout/Spacing
- Panel headers are used in several layouts without consistent alignment or spacing.
- Form action spacing is inconsistent with the grid rhythm.
- Stack of standalone panels (e.g., asset details) can feel tight with no consistent vertical rhythm.

### Typography
- Heading hierarchy is implicit; `h2` sizing and spacing vary by browser defaults.
- Muted/helper text and empty states are the same size as body copy, reducing hierarchy.

### Components
- Buttons and links lack consistent hover/focus feedback; ghost buttons feel flat on hover.
- Tables and timeline items could benefit from clearer hover/active affordance.

### Forms
- Validation errors are red but not visually tied to the input; error spacing varies.
- Inputs lack disabled styling and consistent focus ring behavior.

### States
- No loading state for primary actions.
- Disabled states for buttons and inputs are not visually distinct.

### Mobile
- Map sidebar has a fixed width and can overflow on smaller screens.
- Panel headers with multiple actions can wrap awkwardly without consistent spacing.

### Accessibility
- Focus rings are missing for buttons, links, and `summary` elements.
- Error summaries do not stand out or announce clearly.

## Minimal Change Plan (Priority Order)
1. Add spacing tokens and type hierarchy to normalize headings and helper text sizes.
2. Normalize panel headers: flex alignment, spacing, and consistent divider rhythm.
3. Improve button/link interactions: hover, focus-visible, active, disabled styles.
4. Improve form styling: consistent label spacing, error states, validation summary treatment.
5. Add lightweight loading state for submit buttons (CSS + tiny JS hook).
6. Improve empty state styling to feel intentional without changing layouts.
7. Add small responsive fixes (map sidebar width, header wrapping).
8. Add basic accessibility enhancements (focus rings on nav links, details summary).
