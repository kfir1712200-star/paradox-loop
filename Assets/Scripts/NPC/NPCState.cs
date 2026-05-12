/// <summary>
/// Possible states for an NPC in the game.
/// </summary>
public enum NPCState
{
    Idle,           // Standing still
    Walking,        // Moving to a destination
    Sitting,        // Seated at a location
    Talking,        // In conversation (with player or another NPC)
    Eating,         // At cafeteria/eating area
    Studying,       // At desk/library
    Patrolling,     // Walking a patrol route
    Waiting,        // Waiting at a location
    Reacting,       // Reacting to player/event
    Fleeing,        // Running away
    SpecialEvent    // Cutscene or scripted event
}
