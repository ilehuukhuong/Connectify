export class CreateCallDto {
    callerUsername!: string;
    recipientUsername!: string;
    isVoiceEnabled!: boolean;
    isVideoEnabled!: boolean;
    isScreenSharingEnabled!: boolean;
  }
  export interface Room {
    name: string;
    connections: Connection[];
}

export interface Connection {
    connectionId: string;
    username: string;
}