# UI Standard — Community Health Hub

> **Instructions page v4.0** — created 2026-09-02. Warm sand and clay base
> from the team's reference designs, with the PRD's blood red restored as
> the urgency and blood signal.
> Read by the Designer agent and the frontend coding agent. Governed by the
> **Project Rules** page (root `CLAUDE.md` / `.claude/rules/`).
> This page is a team extension **[EXT]** — it is not part of the base
> toolkit.
>
> Token values here must match `design/component-gallery.html`'s
> `:root{...}` block exactly — that file is the live, running reference and
> wins on any disagreement (see `.claude/agents/designer.md` Inputs).

---

## Design direction

**Who this is for.** Someone in a hospital corridor at 2am on a phone,
possibly on poor signal. Also a hospital admin at a desk managing
inventory. The first person sets the rules; the second adapts to them.

**The organising idea — an urgency gradient.** The product has two
registers and the UI shifts between them.

| Register | Where | Treatment |
|---|---|---|
| Emergency | Blood request, donor response, emergency hub | 52-54px targets, one action per screen, high contrast, short copy |
| Everyday | Wellness, events, profile, inventory | 48px targets, more spacing, calmer weight, room for detail |

Same tokens throughout. Density and weight change, not the system.

**Two colour families, one job each.**

- **Clay `#B06134`** is the brand. Primary actions, navigation, the hero
  panel, everything routine.
- **Blood red `#D32F2F`** is reserved: blood groups, emergency, donate
  actions, destructive controls.

Red appears rarely, which is what lets it read as a signal rather than
decoration. A screen where everything is red tells the reader nothing.
This is the rule that matters most in the whole system — every time you
are tempted to use red for a non-urgent accent, use clay.

**Supersedes the PRD's blue.** The PRD's Wireframe Instructions specify
`#D32F2F` and `#1976D2`. The red is retained exactly; the wellness blue is
replaced by clay and the warm sand base. Update the PRD's colour line on
its next revision — a patch bump under Change Management.

**Type is Plus Jakarta Sans** with a metrically-close system fallback,
loaded with `font-display: swap`. Tabular numerals throughout: blood
groups, distances, unit counts and OTP digits are the densest strings in
the product and they get treated as designed elements.

**Deliberately avoided:** identical rounded cards for every content type,
one border-radius everywhere regardless of hierarchy, gradient washes as
decoration, all-caps micro-labels, and animated entrances on every
section. Radius and elevation encode hierarchy here — a modal is not a
chip.

---

## Tokens

### Colour

```css
:root{
  /* surfaces */
  --sand:        #F6F0E5;  /* page */
  --sand-2:      #EFE7D9;  /* subtle fill, slider track, stat card */
  --cream:       #FDFAF4;  /* card surface */
  --line:        #E6DAC7;
  --line-strong: #D6C6AC;

  /* brand — clay. Routine actions, navigation, hero panels */
  --clay:        #B06134;
  --clay-hover:  #96502A;
  --clay-active: #7E4322;
  --clay-deep:   #8A4620;
  --clay-tint:   #F5E4D3;
  --clay-line:   #E8CDB2;

  /* signal — blood red. Blood groups, emergency, donate, destructive */
  --blood:       #D32F2F;
  --blood-hover: #B92626;
  --blood-active:#9E1B18;
  --blood-deep:  #9E1B18;  /* text on tint */
  --blood-tint:  #FCE9E7;
  --blood-line:  #F5C6C2;

  /* text */
  --ink:         #392A1D;
  --ink-2:       #6F5C46;
  --ink-3:       #A08A70;
  --ink-off:     #BFAE96;  /* disabled */

  /* semantic */
  --leaf:        #4E7A46;  --leaf-tint:  #E8F0E2;  /* verified, eligible */
  --amber:       #8F6A12;  --amber-tint: #F7EDD4;  /* pending, urgent */
  --error:       #A33325;  --error-tint: #F7E4E0;  /* field and form errors */

  --overlay:     rgba(57,42,29,.5);
  --focus:       #B06134;
}
```

**Dark theme** is supported and shipped. Same token names, swapped values
on `[data-theme="dark"]` — clay lifts to `#C36F3C` and blood to `#E5493F`
for contrast on dark surfaces (sand becomes `#15110C`, cream becomes
`#221B14`). Components never reference a theme, only tokens.

**Where each colour is allowed**

| Element | Colour |
|---|---|
| Blood group badge | `--blood-tint` fill, `--blood-deep` text |
| Emergency urgency, card rail, banner | `--blood` |
| Urgent urgency | `--amber` |
| Standard urgency | `--sand-2` / `--ink-2` |
| "Request blood", "Donate", "Respond" on an emergency | `--blood` filled |
| "Save", "Continue", "RSVP", navigation active | `--clay` filled |
| Destructive — withdraw, delete, reject | `--blood` **outlined**, not filled |
| Field and form errors | `--error` |
| Verified, eligible | `--leaf` |
| Elapsed-time ring | `--blood` |

Destructive actions are outlined rather than filled. An outline signals
consequence without shouting, and it keeps solid red meaning "this is an
emergency" rather than "this button is dangerous."

**Rules**

1. `--clay` is the primary/urgency-neutral action colour. `--blood` is
   reserved for blood/emergency/destructive — they are never
   interchangeable.
2. **Red is a signal, not an accent.** If it is not blood, emergency or
   destructive, it is clay.
3. **Colour never carries meaning alone.** Every status, blood group and
   urgency level also carries text. Not optional on this product.
4. Shadows are warm-tinted (`rgba(57,42,29,…)`), never neutral grey. Grey
   shadows on a sand background read as dirt.
5. Body text clears 4.5:1. Large text, icons and UI boundaries clear 3:1.
   White on `--blood` measures 5.0:1 and white on `--clay` measures 4.7:1
   — both pass, but neither has margin, so re-check if you shift either
   value.
6. No colour outside this list. If a screen needs one, add it here first.

### Type

```css
--font: "Plus Jakarta Sans", -apple-system, BlinkMacSystemFont,
        "Segoe UI", Roboto, sans-serif;
```

Loaded with `font-display: swap`. The fallback is metrically close enough
that a failed load degrades rather than breaks — which matters on a
patchy mobile connection.

| Token | Size / line | Weight | Tracking | Use |
|---|---|---|---|---|
| `display` | 34 / 40 | 800 | −0.03em | Emergency screen titles, big stats |
| `title` | 28 / 34 | 800 | −0.025em | Screen title, one per screen |
| `heading` | 22 / 28 | 700 | −0.02em | Section heading |
| `subhead` | 17 / 24 | 700 | 0 | Card title, facility name |
| `body` | 16 / 25 | 400 | 0 | Default body, inputs |
| `label` | 13 / 18 | 600 | 0 | Field labels |
| `caption` | 12.5 / 17 | 400 | 0 | Helper text, distance, timestamps |
| `eyebrow` | 11 / 14 | 700 | 0.06em | Stat card labels only |

**Rules**

- `font-variant-numeric: tabular-nums` on every number — blood groups,
  distances, unit counts, OTP, stats. Figures that shift width as they
  change look broken.
- Inputs at 16px minimum; smaller triggers zoom-on-focus in iOS Safari.
- Body line length caps at 68 characters.
- `eyebrow` is the only uppercase in the system, and only on stat cards.
  Uppercase micro-labels above every heading is the commonest
  generated-page tell.

### Spacing, radius, elevation, motion

```css
--s1:4px;  --s2:8px;  --s3:12px; --s4:16px;
--s5:24px; --s6:32px; --s7:48px; --s8:64px;

/* radius encodes hierarchy — never one value on everything */
--r-xs:8px;    /* tag, small toggle */
--r-sm:12px;   /* input, button, list row */
--r-md:16px;   /* card, banner, tile */
--r-lg:20px;   /* modal, split panel */
--r-xl:28px;   /* bottom sheet */
--r-full:999px;/* pill, avatar, blood group badge */

/* warm-tinted elevation */
--e1:0 1px 2px rgba(57,42,29,.04), 0 2px 6px rgba(57,42,29,.05);
--e2:0 2px 6px rgba(57,42,29,.05), 0 8px 20px rgba(57,42,29,.07);
--e3:0 10px 24px rgba(57,42,29,.10), 0 24px 56px rgba(57,42,29,.14);

--t-fast:130ms cubic-bezier(.2,0,.2,1);
--t-base:220ms cubic-bezier(.2,0,.2,1);
```

Screen padding: `--s4` on mobile, `--s6` from 768px.

All motion respects `prefers-reduced-motion`. When set, transitions become
instant. No scroll-triggered or decorative animation anywhere — the only
continuous motion in the system is the searching-for-donors pulse, and
that exists to show the app is still working.

### Breakpoints

| Name | Width | Notes |
|---|---|---|
| `sm` | 0–639 | Primary target. Design here first |
| `md` | 640–1023 | Tablet, larger phones landscape |
| `lg` | 1024+ | Facility and Admin dashboards |

Individual and Guest flows are mobile-first. Facility and Admin dashboards
are desktop-first and degrade to a single column.

---

## Interaction rules — apply to every component

**Five states, always:** default, hover, focus-visible, active, disabled.
Plus error and loading where they apply. A component missing a
focus state fails the Definition of Done (root `CLAUDE.md`).

**Focus ring**, identical everywhere:

```css
outline: 2px solid var(--focus);
outline-offset: 2px;
border-radius: inherit;
```

Never removed. Use `:focus-visible` so mouse clicks don't show it but
keyboard does.

**Touch targets:** 48×48 minimum, 52-54×52-54 on emergency paths. Applies
to the tappable area, not the visible box — a 20px checkbox still needs a
48px hit area.

**Disabled:** `--ink-off` text, `--sand-2` fill, no shadow,
`cursor: not-allowed`, `aria-disabled`. Never remove a disabled control
from the tab order without a reason.

**Loading:** the component holds its own size. Nothing reflows. A button
that shrinks to fit a spinner moves everything below it.

---

## Actions

### Button

| Variant | Fill | Text | Border | Use |
|---|---|---|---|---|
| `b-blood` | `--blood` | `#fff` | none | Request blood, Donate, Respond to an emergency |
| `b-primary` | `--clay` | `#fff` | none | Save, Continue, RSVP, Verify |
| `b-secondary` | transparent | `--ink` | 1.5px `--line-strong` | Cancel, Back, respond to a standard request |
| `b-ghost` | transparent | `--clay` | none | Inline text actions, resend |
| `b-danger` | transparent | `--blood` | 1.5px `--blood-line` | Withdraw consent, Delete, Reject |
| `b-onclay` | `--cream` | `--blood-deep` | none | Action inside a solid red banner |

| Size | Height | Padding | Type | Use |
|---|---|---|---|---|
| `lg` | 54 | 0 `--s5` | `label` 16px | Emergency paths, primary form submit |
| default | 48 | 0 `--s5` | `label` | Default |
| `sm` | 36 | 0 `--s4` | `caption` 13px | Inside cards and table rows |

Radius `--r-sm` (default/`sm`), `--r-md` (`lg`). Full width on mobile
forms; auto width on desktop.

**States** — hover shifts to `-hover`, active to `-active` with no
transform. Loading replaces the label with a spinner, width held.
Disabled per the global rule.

**One primary per screen.** If two actions look equally important, one of
them isn't.

### Icon button

42×42 visible/hit area, `--r-full`, transparent by default, `--sand-2` on
hover. Always needs `aria-label` — an icon alone is not a label.

### Floating action button

Only on the Individual dashboard for "Request blood". 56×56, `--r-full`,
`--blood`, `--e2`, bottom-right at `--s4`, sits above bottom nav. One per
app.

---

## Inputs

Shared anatomy, top to bottom: **label → control → helper or error**.

Label is always visible in `label` type, `--ink-2`. A placeholder is never
the only label — it disappears on focus and screen readers skip it.
Required fields carry an asterisk *and* the word "Required" in helper
text.

### Text input

Height 50, radius `--r-sm`, 1.5px `--line`, `--sand` fill, `--s4`
horizontal padding, `body` 16px.

| State | Treatment |
|---|---|
| Hover | border `--line-strong` |
| Focus | border `--clay`, `--cream` fill, focus ring |
| Filled | as default |
| Error | border `--error`, helper becomes error message in `--error` with a warning icon |
| Disabled | `--sand-2` fill, `--ink-off` text |
| Read-only | no border, `--sand-2` fill, no focus ring |

Optional leading icon at `--s4`, trailing clear or reveal button as a 32px
icon button.

Validate on blur, never on every keystroke. Re-validate on change once a
field has errored.

### Textarea

Same as text input. Min-height 104, resize vertical only, `--s3`/`--s4`
padding. Character counter in `caption` bottom-right when a limit exists;
turns `--amber` at 90%, `--error` at 100%.

### Select

Matches text input exactly. Trailing chevron, rotates 180° on open in
`--t-fast`.

Menu: `--cream`, `--r-sm`, `--e2`, max-height 320 with scroll, 44px
options, `--clay-tint` on hover, checkmark on the selected item. Above 8
options it becomes searchable.

Blood group select renders each option in `data` (tabular-nums) type.

### Search

Text input with a leading search icon and a trailing clear button. Radius
`--r-full` when standalone in a header, `--r-sm` when in a form. Debounce
300ms. Shows a 16px spinner in the trailing slot while querying.

### OTP input

Six separate boxes, 52×58 each (44×52 under 400px), `--s2` gap, `data`
type at 22px, centred.

Auto-advance on entry, backspace moves back, paste fills all six.
`inputmode="numeric"` and `autocomplete="one-time-code"` — without these,
mobile OTP autofill doesn't work, and this is the first screen every user
meets.

Filled box: border `--clay`, `--cream` fill. Error: all six borders
`--error` with a shake of 200ms, respecting reduced motion. Resend link
below in `b-ghost` button, disabled with a countdown until available.

### Checkbox

19×19 visible, 48×48 hit area, `--r-xs`, 1.5px `--line`/`--line-strong`.
Checked: `--clay` fill, white tick. Label to the right in `body`, whole
row clickable (`.choice` pattern).

Health screening checkboxes get `--s3` vertical spacing and sit on a
`--cream` card each — these carry the eligibility decision and need to be
unmissable.

### Radio

19×19, `--r-full`, otherwise identical to checkbox. Always in a
`role="radiogroup"` with a group label. Arrow keys move within the group.

For 2–4 mutually exclusive options, prefer a segmented control (`.seg`)
over radios — fewer taps, clearer on mobile.

### Toggle

46×28 track, `--r-full`, 22px thumb. Off: `--line-strong`. On: `--clay`.
Transition `--t-fast`.

Use only for immediate settings that take effect at once. For anything
saved with a form, use a checkbox — a toggle implies it already applied.

### Slider — search radius

Specific to blood requests and event discovery, 5–100km.

Track 6px `--sand-2`, filled portion via thumb accent `--clay`, thumb 26px
`--cream` with 3px `--clay` border and `--e1`. Current value shown in the
paired numeric field, not floating above the thumb.

Ticks at 5, 25, 50, 75, 100. Arrow keys step 5km, Page keys step 25.
**Always paired with a text field** showing the exact value — a slider
alone is imprecise, and this number decides how many people get notified.

### Date picker

Trigger is a text input with a trailing calendar icon. Native
`<input type="date">` on mobile — the OS picker is better than anything
we'd build and it's already familiar.

Custom calendar on desktop: `--cream`, `--r-lg`, `--e3`, 7-column grid,
40px cells. Today outlined `--clay`; selected filled `--clay`; disabled
`--ink-off`. Arrow keys move by day, Page by month, Home and End to week
bounds.

Date of birth uses a year-first flow, not a month grid — nobody pages
back 30 years one month at a time.

### Time picker

Native on mobile. Desktop is two scrollable columns, hour and minute,
15-minute steps for events, 1-minute for reminders.

### File upload — facility licence

Drop zone: 2px dashed `--line-strong`, `--r-md`, `--s6` padding, `--sand`
fill. Hover and drag-over: `--clay` border, `--clay-tint` fill.

Content: icon, "Drop your licence here or browse", then accepted formats
and max size in `caption`.

Uploaded file renders as a row (`.filerow`): file icon, name truncated
from the middle, size, progress bar while uploading, then a remove icon
button. Errors appear on the row itself, not as a toast — the user needs
to see which file failed.

Accepts PDF, JPG, PNG. Max 10MB. Validate type and size before upload
starts.

---

## Feedback

### Toast

Bottom-centre on mobile, bottom-right on desktop. Max 420 wide, `--r-sm`,
`--e3`, `--s3`/`--s4` padding.

| Type | Left border / icon colour |
|---|---|
| Success (`ok`) | `--leaf` |
| Error (`err`) | `--error` |
| Info (`info`) | `--clay` |

Enters with a slide/fade from 10px below (`--t-base`). Success dismisses
after 4s, error stays until dismissed. Maximum three stacked; older ones
drop off. Dismiss button always present.

Toasts confirm, they don't explain. **A toast is never the only place an
error appears** — it disappears, and a user who looked away has lost the
message. Field errors go on the field.

Copy uses the same verb as the action: "Publish" produces "Published",
not "Success!"

### Inline alert

For persistent messages inside a page. Full width, semantic tint fill
(`a-info` / `a-warn` / `a-err` / `a-ok`), `--r-sm`, `--s3`/`--s4` padding.
Icon, then title in `label`/bold, then body in `caption`-ish 13px. Optional
action as a `b-ghost` or `b-onclay` button.

Used for: guest session expiring, facility pending verification,
receiver-only status.

### Medical disclaimer

Required on wellness and reminder screens — domain constraint (root
`CLAUDE.md` non-goals: no medical diagnosis/clinical advice).

`--sand-2` fill, `--s4` padding, `caption` type, `--ink-2`, `--r-sm`.
Always visible, never inside a collapsed accordion, never dismissible.

### Modal

Desktop only. Max ~520 wide, `--r-lg`, `--e3`, `--overlay` scrim. Header
with title and close, body, footer with actions right-aligned — secondary
then primary.

Focus traps inside, Escape closes, focus returns to the trigger on close.
Body scroll locks.

### Bottom sheet

Mobile equivalent of a modal. Full width, `--r-xl` on top corners only,
`--e3`, grab handle centred at top.

Enters with a slide. Swipe down or tap the scrim to dismiss. Max height
90vh with internal scroll.

### Confirmation dialog

For destructive or irreversible actions: withdrawing donor consent,
rejecting a facility, deleting a request.

Title states the consequence, not the action — "Withdraw your donor
consent?" not "Are you sure?" Body says exactly what happens. Buttons are
`b-secondary` Cancel and `b-danger` with the literal verb: "Withdraw
consent", never "OK".

### Loaders

**Spinner** — 16 / 24 / 34px (`.spin` / `.spin.m` / `.spin.l`), 2-3px
stroke, `currentColor`, 700ms rotation. Inside buttons and small
containers only.

**Skeleton** — `--sand-2` blocks at `--r-xs`, shimmering left to right
over 1.5s. Use for lists and cards where the shape is known. Match the
real content's shape; a skeleton that doesn't match causes a visible jump
when data lands.

**Progress bar** — 6px, `--r-full`, `--sand-2` track, `--clay` fill.
Determinate for uploads.

**Never a bare full-screen spinner.** Skeletons for lists, inline
spinners for actions, progress bars for uploads.

Anything expected to take over 400ms shows a loading state. Below that,
showing one causes a flash that reads as a glitch.

### Empty state

Centred, `--s7` vertical padding. Icon at 68px in `--ink-3` on a
`--sand-2` circle, title bold ~17px, one line of body in `--ink-2`, then
one primary action.

An empty screen is an invitation to act. "No donors within 25 km. Try
widening your search radius." with a button that widens it.

### Error state

Same layout as empty state. Say what happened and what to do. Never blame
the user, never apologise, never say "Something went wrong" without a
next step. Always offer a retry.

---

## Display

### Card

`--cream`, `--r-md`, 1px `--line`, `--e1`, `--s4` padding, `--s3` gap in a
list.

Interactive cards (`.card.tap`) raise to `--e2` on hover and take the
focus ring. Static cards do neither — if it doesn't respond, it shouldn't
look like it will.

**Donor card:** name, blood group badge, distance in `data`, availability
pill. Contact details appear only after an accepted request.

**Request card:** urgency indicator (left rail + pill + elapsed ring),
blood group, units needed, hospital, distance, time posted, action
button.

### Blood group badge

`--r-full`, `data` type, min 46×46 (36×36 `sm`), `--blood-tint` fill,
`--blood-deep` text.

**Always shows the letters.** Never colour-coded alone. Rare groups get a
small dot indicator plus a text "Rare" tag beside it — the dot is never
the only signal.

### Urgency indicator

| Level | Treatment |
|---|---|
| Standard | `--ink-2` text on `--sand-2`, `--r-full`, label "Standard" |
| Urgent | `--amber` on `--amber-tint`, `--r-full`, label "Urgent" |
| Emergency | `#fff` on `--blood`, `--r-full`, label "Emergency", weight 700 |

The word is mandatory at every level. Card left rail (`--line-strong` /
`--amber` / `--blood`) and the elapsed-time ring reinforce it — three
signals, none alone.

### Status pill

`--r-full`, 12.5px, `--s2`(vert)/`--s3`(horiz) padding, semantic tint fill
with semantic text.

Pending · Verified · Rejected · Eligible · Receiver only · Guest, expires
in *n* days.

### Tag / chip

`--r-full`, 12.5px, `--sand-2` fill, 1px `--line`. Removable variant adds
a small close icon. Filter chips toggle to `--clay-tint` fill /
`--clay-line` border / `--clay-deep` text when active.

### Avatar

44px default, `--r-full`. Initials on `--clay-tint` in `--clay-deep` when
no image. Facility avatars are `--r-sm` square — organisations aren't
people, and the shape says so.

### Table — desktop only

Header row `--sand-2`, 11px uppercase `--ink-2`, 44px. Body rows with 1px
`--line` between, `--sand` on hover.

Numeric columns right-aligned in `data`. Sortable headers show a chevron.

**Mobile: tables become cards.** A horizontally scrolling table on a
phone is unusable. Each row becomes a card with labelled fields.

### Tabs

Underline style. `label`/14px type, `--ink-3` inactive, `--ink` active
with a 2.5px `--clay` underline. Scrolls horizontally on mobile when the
set overflows. Arrow keys move between tabs.

### Accordion

Header with chevron rotating 180° in `--t-fast`. Content `--s4` padding,
expands in `--t-base`. 1px `--line` between items.

Never hide required information in an accordion. Never the medical
disclaimer.

### Stepper — progress

For multi-step registration. Horizontal on desktop (`.steps`: done/now/
todo circles connected by rules), compact "Step 2 of 4" on mobile.
Completed `--leaf` with a tick, current `--clay` filled, upcoming
`--line-strong` outline.

### Tooltip

`--ink` fill, white text, `--r-sm`, small padding, max ~240 wide. Delay on
hover, immediate on focus.

Never the only way to reach information. Touch devices have no hover.

### Divider

1px `--line`. `--s4` margins in a list, `--s5` between sections. Use
spacing first — reach for a divider only when spacing alone doesn't
separate things clearly.

---

## Navigation

### App bar

Mobile: 58px, `--cream`, 1px `--line`. Back or menu on the left, title
centred, one action right. Sticky.

Desktop: taller, title left, actions right, no centring.

### Bottom navigation — mobile

`--cream`, 1px top `--line`, safe-area padding for iOS home indicator.
Three to five items. Icon above 11px label. Active item `--clay`;
inactive `--ink-3`.

Labels are always visible. Icon-only navigation guesses wrong for
first-time users, and most of ours are first-time.

### Side navigation — desktop

`--sand-2` panel, items `--r-sm`, `--cream` fill with `--clay` text +
`--e1` when active.

For Facility and Admin dashboards.

---

## Rules for agents **[EXT]**

1. **Tokens, never raw values.** `var(--blood)`, not `#D32F2F`. In Figma
   these are Variables; in React they are CSS custom properties. An agent
   that writes a hex code has broken the system.
2. **Never invent a token or a component.** If a screen needs something
   not on this page or in `design/component-gallery.html`, stop and ask.
   Adding it here first is the point.
3. **Five states on every interactive element.** Default, hover,
   focus-visible, active, disabled — plus error and loading where they
   apply.
4. **Colour never alone.** Every status, urgency level and blood group
   carries text.
5. **Mobile first.** Build the mobile layout, then adapt upward.
   Exception: Facility and Admin dashboards.
6. **Contrast is measured, not assumed.** 4.5:1 body, 3:1 large text and
   UI boundaries.
7. **Every input has a visible label.** A placeholder is never the only
   label.
8. **Nothing reflows on state change.** Loading and error states hold
   their container's size.
9. **Synthetic data only** in mockups, frames and screenshots. No real
   name, phone number, blood group or medication list, including a team
   member's own.
10. **No clinical content in a design.** No dosage, no symptom guidance,
    no diagnosis (root `CLAUDE.md` non-goals). If a screen seems to need
    it, stop and raise it.
11. **Copy rules.** Buttons name what happens: "Request blood", not
    "Submit". The action keeps its name through the flow — "Publish"
    produces "Published". Sentence case everywhere. Errors explain what
    happened and what to do next.

---

## Design source structure

CHH currently uses **Claude Design**, not Figma (Figma MCP is blocked by a
usage-limit issue — see `.claude/agents/designer.md`). One shared canvas
holds every screen, added to incrementally by the `/design` flow:

| Concept | Where |
|---|---|
| Canonical canvas URL | `design/design-links.md` — Canvas section |
| Per-story artboard log | `design/design-links.md` — Story → Artboard log |
| Live component/token reference | `design/component-gallery.html` (open directly in a browser) |

If/when Figma is unblocked, this section should be replaced with the
`00 Foundations` / `01 Components` / `02 Screens` page structure and a
frame-naming contract (`CHH-F0n — Screen Name`), matching how the
Architect's Jira tickets are keyed.

---

## Related Pages

| Page | Link |
|---|---|
| Project Rules | root `CLAUDE.md`, `.claude/rules/api-standards.md`, `.claude/rules/db-standards.md` |
| PRD: Community Health Hub | PRD-CHH-v2.2 (Confluence) — see root `CLAUDE.md` |
| Component Gallery (live) | `design/component-gallery.html` |
| Design links / canvas | `design/design-links.md` |
