Feature: Group Pre-Order

  Guests invited to a booking can each assemble their own pre-order dishes.
  The booking owner reviews all participant pre-orders together, submits one
  consolidated order to the restaurant, and can remove guest participants.

  Background:
    Given a customer who owns a group booking

  Rule: The owner can view participants and their pre-orders

    @api @smoke
    Scenario: The owner views participants and totals through the service
      When the owner reviews the group pre-order
      Then the owner sees the participants and their dishes
      And the total matches the sum of participant totals

    @ui @smoke
    Scenario: The owner views the group pre-order on the storefront
      When the owner opens the group pre-order page
      Then the owner sees the group pre-order details and participants

    @api @negative
    Scenario: A non-owner cannot view group pre-orders
      When another customer tries to review the group pre-order
      Then access is refused

    @api @negative
    Scenario: Reviewing a non-existent group pre-order is refused
      When someone tries to review a non-existent group pre-order
      Then the guest is told that the group could not be found

  Rule: The owner can submit the consolidated pre-order

    @api @smoke
    Scenario: The owner submits the group pre-order through the service
      When the owner submits the group pre-order
      Then the group pre-order is submitted

    @ui @smoke
    Scenario: The owner submits the group pre-order on the storefront
      Given the owner has opened the group pre-order page
      When the owner submits the pre-order through the form
      Then the storefront confirms the pre-order is sent to the restaurant

    @api @negative
    Scenario: Submitting by someone other than the owner is refused
      When another customer tries to submit the group pre-order
      Then access is refused

  Rule: The owner can remove a guest from the group

    @api @smoke
    Scenario: The owner removes a guest from the group through the service
      When the owner removes a guest from the group
      Then the guest is no longer part of the group

    @ui
    Scenario: The owner removes a guest on the storefront
      Given the owner has opened the group pre-order page
      When the owner removes a guest through the storefront
      Then the guest is removed from the participants list

    @api @negative
    Scenario: Removing a guest that does not belong to the group is refused
      When the owner tries to remove an invalid guest from the group
      Then the removal is refused
