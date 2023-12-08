import { User } from "./user";

export class UserParams {
    gender!: string;
    minAge = 18;
    maxAge = 99;
    pageNumber = 1;
    pageSize = 8;
    orderBy = 'lastActive';
    distance = 100;
    currentLatitude!: number;
    currentLongitude!: number;
    similarity = 2;
    constructor(user: User) {
        if (user.gender) {
            this.gender = user.gender;
        } else {
            this.gender = '';
        }
    }

}