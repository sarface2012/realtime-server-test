'use strict';
const util = require('util');

const tickTime = 1000;
const minimumElapsedTime = 120;

const SendTest1 = 10;
const SendTest2 = 11;

const RecieveTest1 = 31;
const RecieveTest2 = 32;

// The Realtime server session object
var session;

var logger;
var activePlayers = 0;              // Records the number of connected players

var startTime;                      // Records the time the process started

function init(rtSession) {
    session = rtSession;
    logger = session.getLogger();

}


// A simple tick loop example
// Checks to see if a minimum amount of time has passed before seeing if the game has ended
async function tickLoop() {
    const elapsedTime = getTimeInS() - startTime;
    logger.info("Tick... " + elapsedTime + " activePlayers: " + activePlayers);

    // In Tick loop - see if all players have left early after a minimum period of time has passed
    // Call processEnding() to terminate the process and quit
    if ((activePlayers == 0) && (elapsedTime > minimumElapsedTime)) {
        logger.info("All players disconnected. Ending game");
        const outcome = await session.processEnding();
        logger.info("Completed process ending with: " + outcome);
        process.exit(0);
    }
    else {
        setTimeout(tickLoop, tickTime);
    }
}

// Calculates the current time in seconds
function getTimeInS() {
    return Math.round(new Date().getTime() / 1000);
}

function onProcessStarted(args) {
    logger.info(`[onProcessStarted]`);
    return true;
}
function onStartGameSession(gameSession) {
    // Complete any game session set-up
    logger.info(`[onStartGameSession]`);
    // tryDelayExit();
    startTime = getTimeInS();
    tickLoop();
}

// Handle process termination if the process is being terminated by GameLift
// You do not need to call ProcessEnding here
function onProcessTerminate() {
    // Perform any clean up
}

// On Player Connect is called when a player has passed initial validation
// Return true if player should connect, false to reject
function onPlayerConnect(connectMsg) {
    logger.info(`[onPlayerConnect]`);
    return true;
}

// Called when a Player is accepted into the game
function onPlayerAccepted(player) {
    logger.info(`[onPlayerAccepted]`);
    activePlayers++;
}

// On Player Disconnect is called when a player has left or been forcibly terminated
// Is only called for players that actually connected to the server and not those rejected by validation
// This is called before the player is removed from the player list
function onPlayerDisconnect(peerId) {
    logger.info(`[onPlayerDisconnect]`);
    activePlayers--;
    // tryDelayExit();
}

// Return true if the player is allowed to join the group
function onPlayerJoinGroup(groupId, peerId) {
    return true;
}

// Return true if the player is allowed to leave the group
function onPlayerLeaveGroup(groupId, peerId) {
    return true;
}

// Return true if the send should be allowed
function onSendToPlayer(gameMessage) {
    return true;
}

// Return true if the send to group should be allowed
// Use gameMessage.getPayloadAsText() to get the message contents
function onSendToGroup(gameMessage) {
    logger.info(`[onSendToGroup]`);

    return true;
}

// Handle a message to the server
function onMessage(gameMessage) {
    logger.info(`[onMessage]`);
    switch (gameMessage.opCode) {
        case SendTest1: {
            // do operation 1 with gameMessage.payload for example sendToGroup
            const outMessage = session.newTextGameMessage(RecieveTest1, session.getServerId(), gameMessage.payload);
            session.sendGroupMessage(outMessage, -1);
            break;
        }
        case SendTest2: {
            // do operation 1 with gameMessage.payload for example sendToGroup
            const outMessage = session.newTextGameMessage(RecieveTest2, session.getServerId(), gameMessage.payload);
            session.sendGroupMessage(outMessage, -1);
            break;
        }
    }
}

// Return true if the process is healthy
function onHealthCheck() {
    return true;
}

exports.ssExports = {
    init: init,
    onProcessStarted: onProcessStarted,
    onStartGameSession: onStartGameSession,
    onProcessTerminate: onProcessTerminate,
    onPlayerConnect: onPlayerConnect,
    onPlayerAccepted: onPlayerAccepted,
    onPlayerDisconnect: onPlayerDisconnect,
    onPlayerJoinGroup: onPlayerJoinGroup,
    onPlayerLeaveGroup: onPlayerLeaveGroup,
    onSendToPlayer: onSendToPlayer,
    onSendToGroup: onSendToGroup,
    onMessage: onMessage,
    onHealthCheck: onHealthCheck
};
