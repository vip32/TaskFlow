---
name: requirements-engineering
description: Transform vague feature ideas into lightweight, testable requirements using user stories and short acceptance criteria. Use when clarifying scope, defining expected behavior, capturing edge cases, and producing decision-ready requirements without heavy specification overhead.
---

# Requirements Engineering

Capture what needs to be built before diving into design. This skill produces lightweight, testable requirements using user stories and short acceptance criteria.

## When to Use This Skill

Use requirements engineering when:
- Starting any new feature or project
- Clarifying ambiguous stakeholder requests
- Creating acceptance criteria for user stories
- Documenting system behavior for testing
- Ensuring all team members share understanding

## Lightweight Format

Use simple, consistent statements that are easy to review and test.

**User Story**
```text
As a [role], I want [capability], so that [benefit].
```

**Short Acceptance Criteria**
```text
When [action], then [outcome].
Given [context], when [action], then [outcome].
```

Each story should have 3-5 acceptance criteria and at least one edge or error criterion.

## Step-by-Step Process

### Step 1: Capture User Stories

Format: **As a [role], I want [feature], so that [benefit]**

Focus on:
- Who is the user? (role)
- What do they want to accomplish? (capability)
- Why does it matter? (benefit/value)

### Step 2: Add Short Acceptance Criteria

For each story, add 3-5 short, testable criteria.

Rules:
- Use observable outcomes
- Keep each criterion focused on one behavior
- Include at least one error or edge case

**Example:**
```text
1. Given the user is signed in, when they add a valid card, then the card is saved.
2. When they add an invalid card number, then an inline validation message is shown.
3. When they open checkout with saved cards, then a saved-card list is displayed.
4. When they delete a saved card, then it is removed from the list.
```

### Step 3: Add Edge and Error Cases

Cover failure paths in plain language:
- Invalid input
- Empty states
- Boundary values
- Permission or access failures
- Timeouts or external dependency failures

### Step 4: Validate Requirements

Use this checklist:

**Completeness:**
- [ ] Key user roles identified and covered
- [ ] Normal flow scenarios covered
- [ ] Edge and error cases included
- [ ] Scope boundaries are explicit

**Clarity:**
- [ ] Criteria use plain, precise language
- [ ] Ambiguous words are avoided or defined
- [ ] Outcomes are observable

**Consistency:**
- [ ] Story and criteria format is consistent
- [ ] Terminology is consistent across sections
- [ ] No contradictory behaviors

**Testability:**
- [ ] Every criterion can be verified
- [ ] Inputs/contexts and expected outcomes are stated
- [ ] Normal and error paths are both testable

## Common Mistakes to Avoid

### Mistake 1: Vague Requirements
**Bad:** "System should be fast"
**Good:** "When a user submits search, then results appear within 2 seconds."

### Mistake 2: Implementation Details
**Bad:** "System shall use Redis for caching"
**Good:** "When users request frequently accessed data, then the response is returned quickly from cached data."

### Mistake 3: Missing Error Cases
**Bad:** Only documenting happy path
**Good:** Include at least one invalid input or failure scenario per story.

### Mistake 4: Untestable Requirements
**Bad:** "System should be user-friendly"
**Good:** "When a new user completes onboarding, then they can reach the dashboard in at most 3 clicks."

### Mistake 5: Conflicting Requirements
**Bad:** Requirements that contradict each other
**Good:** Review stories together and resolve conflicts before design.

## Examples

### Example 1: File Upload Feature

```markdown
**User Story:** As a user, I want to upload files, so that I can share documents with my team.

**Acceptance Criteria:**
1. Given the user is authenticated, when they select a supported file up to 10MB, then the upload starts.
2. When they select a file larger than 10MB, then a "file too large (max 10MB)" error appears.
3. When they select an unsupported file type, then an "unsupported format" error appears with allowed types.
4. When upload is in progress, then progress is shown as a percentage.
5. When upload completes successfully, then a success message with the uploaded file link is shown.
6. When upload fails due to network issues, then a retry option is shown.

**Supported File Types:** PDF, DOC, DOCX, XLS, XLSX, PNG, JPG, GIF
**Maximum File Size:** 10MB
```

### Example 2: Search Feature

```markdown
**User Story:** As a customer, I want to search products, so that I can find items quickly.

**Acceptance Criteria:**
1. When the customer enters a search term, then matching products are shown.
2. When results are found, then the result count is displayed.
3. When no results are found, then a "no products found" message with suggestions is displayed.
4. When the customer submits an empty search, then a validation message is shown.
5. When results exceed 20 items, then pagination is shown with 20 items per page.
6. When the customer searches, then results are returned within 2 seconds.

**Search Fields:** Product name, description, category, SKU
**Minimum Search Length:** 2 characters
```

## Requirements Document Template

```markdown
# Requirements Document: [Feature Name]

## Overview
[Short description of the feature and why it exists]

## Scope
- In scope: [items included]
- Out of scope: [items excluded]

## User Roles
- [Role 1]: [Description of this user type]
- [Role 2]: [Description of this user type]

## User Stories

### Story 1: [Name]
As a [role], I want [capability], so that [benefit].

Acceptance Criteria:
1. Given [context], when [action], then [outcome].
2. When [action], then [outcome].
3. When [error or edge case], then [outcome].

### Story 2: [Name]
As a [role], I want [capability], so that [benefit].

Acceptance Criteria:
1. ...
2. ...
3. ...

## Non-Functional Notes
- Performance: [response targets]
- Security: [access and data expectations]
- Accessibility: [standards and constraints]

## Out of Scope
- [Items explicitly not included in this feature]

## Open Questions
- [Questions that need stakeholder input]
```

## Next Steps

After completing requirements:
1. Review with stakeholders for accuracy
2. Get explicit approval before proceeding
3. Move to design phase to create technical architecture
4. Use requirements as foundation for acceptance testing
