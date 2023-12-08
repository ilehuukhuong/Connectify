export interface User {
    username: string;
    token: string;
    photoUrl: string;
    knownAs: string;
    gender: string;
    roles: string[];
    isBlocked: boolean;
    isDeleted: string;
}