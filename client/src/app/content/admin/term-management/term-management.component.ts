import { Component, OnInit, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { ToastrService } from 'ngx-toastr';
import { Term, Terms } from 'src/app/common/_models/term';
import { AdminService } from 'src/app/common/_services/admin.service';

@Component({
  selector: 'app-term-management',
  templateUrl: './term-management.component.html',
  styleUrls: ['./term-management.component.css']
})
export class TermManagementComponent implements OnInit {
  termList: Terms[] | undefined;
  term: Term[] | undefined;
  createForm!: FormGroup;
  termForm!: FormGroup;
  bsModalRef!: BsModalRef;
  constructor(private adminService: AdminService, private fb: FormBuilder,
    private toastr: ToastrService,
    private modalService: BsModalService) { }

  ngOnInit(): void {
    this.getAllTermList();
    this.initializeCreateForm();
  }
  getAllTermList() {
    this.adminService.getAllTermList().subscribe(response => {
      this.termList = response;
      console.log(this.termList);
    });
  }
  initializeCreateForm() {
    this.createForm = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required]
    });
  }
  initializeTermForm() {
    this.termForm = this.fb.group({
      name: [this.term, Validators.required],
    });
  }
  openModal(template: TemplateRef<any>) {
    this.bsModalRef = this.modalService.show(template);
  }
  closeModal() {
    this.bsModalRef.hide();
  }
  createTermList() {
    if (this.createForm.valid) {
      const newTerm = {
        name: this.createForm.get('name')!.value,
        description: this.createForm.get('description')!.value
      };
      this.adminService.createTermList(newTerm.name, newTerm.description).subscribe({
        next: (response: Terms) => {
          this.toastr.success('Term List created successfully');
          // Đợi 2 giây sau đó gọi lại getAllTermList
          setTimeout(() => {
            this.getAllTermList();
          }, 1500);
          this.termList?.push(response);
          this.createForm.reset();
          this.closeModal();
        },
        error: (err: any) => {
          this.toastr.error('Error creating Term List');
          console.error(err);
        }
      });
    } else {
      this.toastr.error('Please fill in all required fields');
    }
  }
  

  onDelete(listId: number) {
    this.adminService.deleteTermList(listId).subscribe(response => {
      this.termList = this.termList?.filter(x => x.Id !== listId);
      this.toastr.success('Deleted successfully!');
    }, error => {
      this.toastr.error(error);
    });
  }
}
