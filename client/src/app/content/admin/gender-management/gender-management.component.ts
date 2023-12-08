import { Component, OnInit, TemplateRef } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Genders } from 'src/app/common/_models/gender';
import { AdminService } from 'src/app/common/_services/admin.service';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-gender-management',
  templateUrl: './gender-management.component.html',
  styleUrls: ['./gender-management.component.css']
})
export class GenderManagementComponent implements OnInit {
  genders: Genders[] | undefined;
  selectedGender!: Genders;
  editForm!: FormGroup;
  bsModalRef!: BsModalRef;
  createForm!: FormGroup;
  message: string = '';
  genderForm!: FormGroup;
  gender!: Genders;
  constructor(private adminService: AdminService, private modalService: BsModalService, private toastr: ToastrService) { }

  ngOnInit(): void {
    this.loadGenders();
    this.editForm = new FormGroup({
      'name': new FormControl(null, Validators.required)
    });
    this.genderForm = new FormGroup({
      'name': new FormControl(null, Validators.required)
    });
  }
  loadGenders() {
    this.adminService.getGenders().subscribe(genders => {
      this.genders = genders.sort((a, b) => a.id - b.id);
    });
  }
  //Create Gender
  openCreateGendersModal(template: TemplateRef<any>) {
    this.bsModalRef = this.modalService.show(template);
  }
  createGender() {
    if (this.genderForm.invalid) {
      return;
    }
    this.gender = this.genderForm.value;
    this.adminService.createGender(this.gender).subscribe(response => {
      this.message = 'Create new gender successfully';
      this.toastr.success(this.message, 'Success');
      this.loadGenders();
      this.bsModalRef.hide();
      this.genderForm.reset();
    });
  }
  closeModal() {
    this.bsModalRef.hide();
  }
  //Edit Gender
  openEditGendersModal(template: TemplateRef<any>, gender: Genders) {
    this.selectedGender = gender;
    this.editForm.setValue({
      'name': this.selectedGender.name
    });
    this.bsModalRef = this.modalService.show(template);
  }
  updateGender() {
    if (this.editForm.invalid) {
      return;
    }
    const updatedGender: Genders = {
      id: this.selectedGender.id,
      name: this.editForm.value.name
    };
    this.adminService.updateGender(updatedGender, this.selectedGender.id).subscribe(response => {
      console.log(response);
      this.message = 'Edit gender successfully';
      this.toastr.success(this.message, 'Success');
      this.loadGenders();
      this.bsModalRef.hide();
    });
  }
  //Delete gender
  openDeleteGendersModal(template: TemplateRef<any>, gender: Genders) {
    this.selectedGender = gender;
    this.bsModalRef = this.modalService.show(template, { class: 'modal-sm' });
  }
  confirmDeleteGender(): void {
    this.adminService.deleteGender(this.selectedGender.id).subscribe(() => {
      this.message = 'Delete gender successfully';
      this.toastr.success(this.message, 'Success');
      this.loadGenders();
      this.bsModalRef.hide();
    });
  }
  declineDeleteGender(): void {
    this.bsModalRef.hide();
  }
 
}
