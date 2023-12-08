import { Component, OnInit, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { ToastrService } from 'ngx-toastr';
import { LookingForsDto } from 'src/app/common/_models/lookingForsDto';
import { AdminService } from 'src/app/common/_services/admin.service';

@Component({
  selector: 'app-lookingfor-management',
  templateUrl: './lookingfor-management.component.html',
  styleUrls: ['./lookingfor-management.component.css']
})
export class LookingforManagementComponent implements OnInit {
  lookingFors: LookingForsDto[] = [];
  searchForm!: FormGroup;
  updateForm!: FormGroup;
  selectedLookingFor!: LookingForsDto;
  bsModalRef!: BsModalRef;
  createForm!: FormGroup;

  constructor(private adminService: AdminService,
    private fb: FormBuilder,
    private toastr: ToastrService,
    private modalService: BsModalService) { }

  ngOnInit() {
    this.initializeSearchForm();
    this.initializeUpdateForm();
    this.initializeCreateForm();
  }
  
  //search
  initializeSearchForm() {
    this.searchForm = this.fb.group({
      name: ['']
    });
  }
  onSearch() {
    const searchValue = this.searchForm.get('name')!.value;
    if (searchValue.trim() === '') {
      this.toastr.error('Please fill in the information');
      return;
    }
    this.loadLookingFors(searchValue);
  }
  loadLookingFors(name?: string) {
    this.adminService.getLookingFors(name).subscribe(response => {
      this.lookingFors = response;
    }, error => {
      this.toastr.error(error);
    });
  }

  //create
  initializeCreateForm() {
    this.createForm = this.fb.group({
      name: ['', Validators.required]
    });
  }
  openCreateModal(template: TemplateRef<any>) {
    this.bsModalRef = this.modalService.show(template);
  }
  createLookingFor() {
    const lookingforText = this.createForm.value.name;
    if (lookingforText.length > 200) {
      this.toastr.error('Your information is too long. Please limit it to 200 characters.');
      return;
    }
    const createValue = this.createForm.get('name')!.value;
    if (createValue.trim() === '') {
      this.toastr.error('Please fill in the information');
      return;
    }
    if (this.createForm.valid) {
      this.adminService.createLookingFor(this.createForm.value).subscribe(response => {
        this.toastr.success('Created successfully');
        this.createForm.reset(this.createForm.value);
        this.closeModal();
      }, error => {
        if (error.statusCode === 500) {
          this.toastr.error('There was an error updating your introduction. Please try again later.');
        } else {
          this.toastr.error(error);
        }
      });
    }
  }

  //update
  initializeUpdateForm() {
    this.updateForm = this.fb.group({
      name: ['']
    });
  }

  openUpdateModal(lookingFor: LookingForsDto, template: TemplateRef<any>) {
    this.selectedLookingFor = lookingFor;
    this.updateForm.patchValue(lookingFor);
    this.bsModalRef = this.modalService.show(template);
  }
  updateLookingFor() {
    const lookingforText = this.updateForm.value.name;
    if (lookingforText.length > 200) {
      this.toastr.error('Your information is too long. Please limit it to 200 characters.');
      return;
    }
    const updateValue = this.updateForm.get('name')!.value;
    if (updateValue.trim() === '') {
      this.toastr.error('Please fill in the information');
      return;
    }
    if (this.updateForm.valid) {
      const updatedLookingFor: LookingForsDto = {
        ...this.selectedLookingFor,
        ...this.updateForm.value
      };
      this.adminService.updateLookingFor(updatedLookingFor).subscribe(response => {
        const index = this.lookingFors.findIndex(lf => lf.id === updatedLookingFor.id);
        if (index !== -1) {
          this.lookingFors[index] = updatedLookingFor;
        }
        this.toastr.success('Updated successfully!');
        this.closeModal();
      }, error => {
        if (error.statusCode === 500) {
          this.toastr.error('There was an error updating your introduction. Please try again later.');
        } else {
          error
        }
      });
    }
  }
  //delete
  onDelete(id: number) {
    this.adminService.deleteLookingFor(id).subscribe(response => {
      this.lookingFors = this.lookingFors.filter(lf => lf.id !== id);
      this.toastr.success('Deleted successfully!');
    }, error => {
      this.toastr.error(error);
    });
  }
  //close modal
  closeModal() {
    this.bsModalRef.hide();
  }
}
